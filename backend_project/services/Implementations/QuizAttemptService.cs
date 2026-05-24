using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.QuizAttempt;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class QuizAttemptService : IQuizAttemptService
{
    private readonly ApplicationDbContext _context;
    private readonly ICertificateService _certificateService;

    public QuizAttemptService(ApplicationDbContext context, ICertificateService certificateService)
    {
        _context = context;
        _certificateService = certificateService;
    }

    public async Task<QuizAttemptResponseDto> StartAttemptAsync(Guid quizId, Guid enrollmentId)
    {
        var quiz = await _context.Quizzes
            .AsNoTracking()
            .Include(q => q.Questions.OrderBy(qn => qn.Position))
                .ThenInclude(qn => qn.Options.OrderBy(o => o.Position))
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz == null)
            throw new KeyNotFoundException("Quiz not found.");

        var enrollment = await _context.Enrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == enrollmentId);

        if (enrollment == null)
            throw new KeyNotFoundException("Enrollment not found.");

        var previousAttempts = await _context.QuizAttempts
            .CountAsync(a => a.QuizId == quizId && a.EnrollmentId == enrollmentId);

        if (quiz.MaxAttempts.HasValue && previousAttempts >= quiz.MaxAttempts.Value)
            throw new InvalidOperationException("Maximum number of attempts reached for this quiz.");

        var attempt = new QuizAttempt
        {
            Id = Guid.NewGuid(),
            QuizId = quizId,
            EnrollmentId = enrollmentId,
            Status = QuizAttemptStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            AttemptNumber = previousAttempts + 1,
            Score = 0,
            MaxScore = quiz.Questions.Sum(q => q.Points)
        };

        _context.QuizAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        var questions = quiz.ShuffleQuestions
            ? quiz.Questions.OrderBy(_ => Guid.NewGuid()).ToList()
            : quiz.Questions.ToList();

        return new QuizAttemptResponseDto
        {
            Id = attempt.Id,
            EnrollmentId = attempt.EnrollmentId,
            QuizId = attempt.QuizId,
            AttemptNumber = attempt.AttemptNumber,
            StartedAt = attempt.StartedAt,
            Status = QuizAttemptStatus.InProgress.ToString(),
            ScorePercentage = 0,
            IsPassed = false
        };
    }

    public async Task<QuizResultDto> SubmitAttemptAsync(Guid attemptId, SubmitAttemptDto submitDto)
    {
        var attempt = await _context.QuizAttempts
            .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                    .ThenInclude(qn => qn.Options)
            .Include(a => a.UserAnswers)
            .FirstOrDefaultAsync(a => a.Id == attemptId);

        if (attempt == null)
            throw new KeyNotFoundException("Quiz attempt not found.");

        if (attempt.Status != QuizAttemptStatus.InProgress)
            throw new InvalidOperationException("This attempt has already been submitted.");

        attempt = await CheckAndAutoSubmitExpiredAsync(attempt);
        if (attempt.Status != QuizAttemptStatus.InProgress)
        {
            var expiredResult = await GetAttemptResultAsync(attemptId);
            expiredResult.IsAutoSubmitted = true;
            return expiredResult;
        }

        var totalScore = 0;
        var maxScore = attempt.Quiz.Questions.Sum(q => q.Points);

        foreach (var answerDto in submitDto.Answers)
        {
            var question = attempt.Quiz.Questions
                .FirstOrDefault(q => q.Id == answerDto.QuestionId);

            if (question == null) continue;

            var isCorrect = false;
            var pointsEarned = 0;

            if (answerDto.SelectedOptionId.HasValue)
            {
                var selectedOption = question.Options
                    .FirstOrDefault(o => o.Id == answerDto.SelectedOptionId.Value);

                if (selectedOption != null)
                {
                    isCorrect = selectedOption.IsCorrect;
                    pointsEarned = isCorrect ? question.Points : 0;
                }
            }

            totalScore += pointsEarned;

            var userAnswer = new UserAnswer
            {
                Id = Guid.NewGuid(),
                AttemptId = attemptId,
                QuestionId = answerDto.QuestionId,
                SelectedOptionId = answerDto.SelectedOptionId,
                AnswerText = answerDto.AnswerText,
                IsCorrect = isCorrect,
                PointsEarned = pointsEarned
            };

            _context.UserAnswers.Add(userAnswer);
        }

        attempt.Score = totalScore;
        attempt.MaxScore = maxScore;
        attempt.Status = QuizAttemptStatus.Graded;
        attempt.SubmittedAt = DateTime.UtcNow;
        attempt.TimeTakenSeconds = (int)(DateTime.UtcNow - attempt.StartedAt).TotalSeconds;

        await _context.SaveChangesAsync();

        return await BuildResultDto(attempt, true);
    }

    public async Task<QuizResultDto> GetAttemptResultAsync(Guid attemptId)
    {
        var attempt = await _context.QuizAttempts
            .AsNoTracking()
            .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                    .ThenInclude(qn => qn.Options)
            .Include(a => a.UserAnswers)
            .FirstOrDefaultAsync(a => a.Id == attemptId);

        if (attempt == null)
            throw new KeyNotFoundException("Quiz attempt not found.");

        return await BuildResultDto(attempt, false);
    }

    public async Task<IEnumerable<QuizAttemptResponseDto>> GetUserAttemptsForQuizAsync(Guid quizId, Guid enrollmentId)
    {
        return await _context.QuizAttempts
            .AsNoTracking()
            .Where(a => a.QuizId == quizId && a.EnrollmentId == enrollmentId)
            .OrderByDescending(a => a.StartedAt)
            .Select(a => new QuizAttemptResponseDto
            {
                Id = a.Id,
                EnrollmentId = a.EnrollmentId,
                QuizId = a.QuizId,
                AttemptNumber = a.AttemptNumber,
                StartedAt = a.StartedAt,
                SubmittedAt = a.SubmittedAt,
                Status = a.Status.ToString(),
                ScorePercentage = a.MaxScore > 0
                    ? Math.Round((decimal)a.Score / a.MaxScore * 100, 2)
                    : 0,
                IsPassed = a.MaxScore > 0
                    && (decimal)a.Score / a.MaxScore * 100 >= a.Quiz.PassingScorePercent
            })
            .ToListAsync();
    }

    private async Task<QuizAttempt> CheckAndAutoSubmitExpiredAsync(QuizAttempt attempt)
    {
        if (attempt.Quiz.DurationMinutes.HasValue)
        {
            var expiryTime = attempt.StartedAt.AddMinutes(attempt.Quiz.DurationMinutes.Value);
            if (DateTime.UtcNow >= expiryTime)
            {
                attempt.Status = QuizAttemptStatus.Submitted;
                attempt.SubmittedAt = DateTime.UtcNow;
                attempt.TimeTakenSeconds = attempt.Quiz.DurationMinutes.Value * 60;

                var maxScore = attempt.Quiz.Questions.Sum(q => q.Points);

                foreach (var existingAnswer in attempt.UserAnswers)
                {
                    var question = attempt.Quiz.Questions
                        .FirstOrDefault(q => q.Id == existingAnswer.QuestionId);
                    if (question != null)
                    {
                        existingAnswer.IsCorrect = existingAnswer.SelectedOptionId.HasValue
                            && question.Options.Any(o => o.Id == existingAnswer.SelectedOptionId.Value && o.IsCorrect);
                        existingAnswer.PointsEarned = existingAnswer.IsCorrect ? question.Points : 0;
                    }
                }

                attempt.Score = attempt.UserAnswers.Sum(ua => ua.PointsEarned);
                attempt.MaxScore = maxScore;

                await _context.SaveChangesAsync();
            }
        }

        return attempt;
    }

    private async Task<QuizResultDto> BuildResultDto(QuizAttempt attempt, bool isNewSubmission)
    {
        var showResults = isNewSubmission || (attempt.Quiz.ShowResultsImmediately && attempt.Status == QuizAttemptStatus.Graded);

        var passed = attempt.MaxScore > 0
            && (decimal)attempt.Score / attempt.MaxScore * 100 >= attempt.Quiz.PassingScorePercent;

        if (passed && isNewSubmission)
        {
            var contentProgressService = new ContentProgressService(_context, _certificateService);
            await contentProgressService.MarkCompletedAsync(
                attempt.EnrollmentId, attempt.QuizId, ContentType.Quiz);
        }

        var result = new QuizResultDto
        {
            Id = attempt.Id,
            EnrollmentId = attempt.EnrollmentId,
            QuizId = attempt.QuizId,
            AttemptNumber = attempt.AttemptNumber,
            StartedAt = attempt.StartedAt,
            SubmittedAt = attempt.SubmittedAt,
            Status = attempt.Status.ToString(),
            ScorePercentage = attempt.MaxScore > 0
                ? Math.Round((decimal)attempt.Score / attempt.MaxScore * 100, 2)
                : 0,
            IsPassed = passed
        };

        if (showResults)
        {
            result.Answers = attempt.UserAnswers.Select(ua =>
            {
                var question = attempt.Quiz.Questions
                    .FirstOrDefault(q => q.Id == ua.QuestionId);

                return new AnswerResultDto
                {
                    QuestionId = ua.QuestionId,
                    SelectedOptionId = ua.SelectedOptionId,
                    AnswerText = ua.AnswerText,
                    IsCorrect = ua.IsCorrect,
                    EarnedPoints = ua.PointsEarned
                };
            }).ToList();
        }

        return result;
    }
}
