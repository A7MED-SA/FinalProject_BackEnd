using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Domain.Interfaces;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Athary.Infrastructure.Services.Courses;

public sealed class QuizAttemptService : IQuizAttemptService
{
    private readonly ApplicationDbContext _context;
    private readonly IRepository<QuizAttempt> _attemptRepo;
    private readonly IRepository<UserAnswer> _userAnswerRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IContentProgressService _contentProgressService;
    private readonly ILogger<QuizAttemptService> _logger;

    public QuizAttemptService(
        ApplicationDbContext context,
        IRepository<QuizAttempt> attemptRepo,
        IRepository<UserAnswer> userAnswerRepo,
        IUnitOfWork unitOfWork,
        IContentProgressService contentProgressService,
        ILogger<QuizAttemptService> logger)
    {
        _context = context;
        _attemptRepo = attemptRepo;
        _userAnswerRepo = userAnswerRepo;
        _unitOfWork = unitOfWork;
        _contentProgressService = contentProgressService;
        _logger = logger;
    }

    public async Task<QuizAttemptResponseDto> StartAttemptAsync(Guid quizId, Guid enrollmentId, CancellationToken cancellationToken = default)
    {
        var quiz = await _context.Quizzes
            .AsNoTracking()
            .Include(q => q.Questions.OrderBy(qn => qn.Position))
                .ThenInclude(qn => qn.Options.OrderBy(o => o.Position))
            .FirstOrDefaultAsync(q => q.Id == quizId, cancellationToken)
            ?? throw new KeyNotFoundException("Quiz not found.");

        var enrollment = await _context.Enrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == enrollmentId, cancellationToken)
            ?? throw new KeyNotFoundException("Enrollment not found.");

        var previousAttempts = await _context.QuizAttempts
            .CountAsync(a => a.QuizId == quizId && a.EnrollmentId == enrollmentId, cancellationToken);

        if (quiz.MaxAttempts.HasValue && previousAttempts >= quiz.MaxAttempts.Value)
            throw new InvalidOperationException("Maximum number of attempts reached for this quiz.");

        var attempt = new QuizAttempt
        {
            QuizId = quizId,
            EnrollmentId = enrollmentId,
            Status = QuizAttemptStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            AttemptNumber = previousAttempts + 1,
            Score = 0,
            MaxScore = quiz.Questions.Sum(q => q.Points)
        };

        await _attemptRepo.AddAsync(attempt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Quiz attempt {AttemptId} started for quiz {QuizId}, enrollment {EnrollmentId}", attempt.Id, quizId, enrollmentId);

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

    public async Task<QuizResultDto> SubmitAttemptAsync(Guid attemptId, SubmitAttemptDto submitDto, CancellationToken cancellationToken = default)
    {
        var attempt = await _context.QuizAttempts
            .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                    .ThenInclude(qn => qn.Options)
            .Include(a => a.UserAnswers)
            .FirstOrDefaultAsync(a => a.Id == attemptId, cancellationToken)
            ?? throw new KeyNotFoundException("Quiz attempt not found.");

        if (attempt.Status != QuizAttemptStatus.InProgress)
            throw new InvalidOperationException("This attempt has already been submitted.");

        attempt = await CheckAndAutoSubmitExpiredAsync(attempt, cancellationToken);
        if (attempt.Status != QuizAttemptStatus.InProgress)
        {
            var expiredResult = await GetAttemptResultAsync(attemptId, cancellationToken);
            return expiredResult with { IsAutoSubmitted = true };
        }

        var totalScore = 0;
        var maxScore = attempt.Quiz.Questions.Sum(q => q.Points);

        foreach (var answerDto in submitDto.Answers)
        {
            var question = attempt.Quiz.Questions
                .FirstOrDefault(q => q.Id == answerDto.QuestionId);

            if (question is null) continue;

            var isCorrect = false;
            var pointsEarned = 0;

            if (answerDto.SelectedOptionId.HasValue)
            {
                var selectedOption = question.Options
                    .FirstOrDefault(o => o.Id == answerDto.SelectedOptionId.Value);

                if (selectedOption is not null)
                {
                    isCorrect = selectedOption.IsCorrect;
                    pointsEarned = isCorrect ? question.Points : 0;
                }
            }

            totalScore += pointsEarned;

            var userAnswer = new UserAnswer
            {
                AttemptId = attemptId,
                QuestionId = answerDto.QuestionId,
                SelectedOptionId = answerDto.SelectedOptionId,
                AnswerText = answerDto.AnswerText,
                IsCorrect = isCorrect,
                PointsEarned = pointsEarned
            };

            await _userAnswerRepo.AddAsync(userAnswer, cancellationToken);
        }

        attempt.Score = totalScore;
        attempt.MaxScore = maxScore;
        attempt.Status = QuizAttemptStatus.Graded;
        attempt.SubmittedAt = DateTime.UtcNow;
        attempt.TimeTakenSeconds = (int)(DateTime.UtcNow - attempt.StartedAt).TotalSeconds;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Quiz attempt {AttemptId} submitted with score {Score}/{MaxScore}", attemptId, totalScore, maxScore);

        return await BuildResultDto(attempt, true, cancellationToken);
    }

    public async Task<QuizResultDto> GetAttemptResultAsync(Guid attemptId, CancellationToken cancellationToken = default)
    {
        var attempt = await _context.QuizAttempts
            .AsNoTracking()
            .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                    .ThenInclude(qn => qn.Options)
            .Include(a => a.UserAnswers)
            .FirstOrDefaultAsync(a => a.Id == attemptId, cancellationToken)
            ?? throw new KeyNotFoundException("Quiz attempt not found.");

        return await BuildResultDto(attempt, false, cancellationToken);
    }

    public async Task<IEnumerable<QuizAttemptResponseDto>> GetUserAttemptsForQuizAsync(Guid quizId, Guid enrollmentId, CancellationToken cancellationToken = default)
    {
        return await _context.QuizAttempts
            .AsNoTracking()
            .Include(a => a.Quiz)
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
            .ToListAsync(cancellationToken);
    }

    private async Task<QuizAttempt> CheckAndAutoSubmitExpiredAsync(QuizAttempt attempt, CancellationToken cancellationToken)
    {
        if (!attempt.Quiz.DurationMinutes.HasValue)
            return attempt;

        var expiryTime = attempt.StartedAt.AddMinutes(attempt.Quiz.DurationMinutes.Value);
        if (DateTime.UtcNow < expiryTime)
            return attempt;

        attempt.Status = QuizAttemptStatus.Submitted;
        attempt.SubmittedAt = DateTime.UtcNow;
        attempt.TimeTakenSeconds = attempt.Quiz.DurationMinutes.Value * 60;

        var maxScore = attempt.Quiz.Questions.Sum(q => q.Points);

        foreach (var existingAnswer in attempt.UserAnswers)
        {
            var question = attempt.Quiz.Questions
                .FirstOrDefault(q => q.Id == existingAnswer.QuestionId);
            if (question is not null)
            {
                existingAnswer.IsCorrect = existingAnswer.SelectedOptionId.HasValue
                    && question.Options.Any(o => o.Id == existingAnswer.SelectedOptionId.Value && o.IsCorrect);
                existingAnswer.PointsEarned = existingAnswer.IsCorrect ? question.Points : 0;
            }
        }

        attempt.Score = attempt.UserAnswers.Sum(ua => ua.PointsEarned);
        attempt.MaxScore = maxScore;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return attempt;
    }

    private async Task<QuizResultDto> BuildResultDto(QuizAttempt attempt, bool isNewSubmission, CancellationToken cancellationToken)
    {
        var showResults = isNewSubmission
            || (attempt.Quiz.ShowResultsImmediately && attempt.Status == QuizAttemptStatus.Graded);

        var passed = attempt.MaxScore > 0
            && (decimal)attempt.Score / attempt.MaxScore * 100 >= attempt.Quiz.PassingScorePercent;

        if (passed && isNewSubmission)
        {
            await _contentProgressService.MarkCompletedAsync(
                attempt.EnrollmentId, attempt.QuizId, ContentType.Quiz, cancellationToken);
        }

        var answers = showResults
            ? attempt.UserAnswers.Select(ua =>
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
            }).ToList()
            : new List<AnswerResultDto>();

        return new QuizResultDto
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
            IsPassed = passed,
            Answers = answers
        };
    }
}
