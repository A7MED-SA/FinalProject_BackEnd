using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Domain.Interfaces;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Athary.Infrastructure.Services.Courses;

public sealed class QuizService : IQuizService
{
    private readonly ApplicationDbContext _context;
    private readonly IRepository<Quiz> _quizRepo;
    private readonly IRepository<Question> _questionRepo;
    private readonly IRepository<Option> _optionRepo;
    private readonly IRepository<SectionItem> _sectionItemRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<QuizService> _logger;

    public QuizService(
        ApplicationDbContext context,
        IRepository<Quiz> quizRepo,
        IRepository<Question> questionRepo,
        IRepository<Option> optionRepo,
        IRepository<SectionItem> sectionItemRepo,
        IUnitOfWork unitOfWork,
        ILogger<QuizService> logger)
    {
        _context = context;
        _quizRepo = quizRepo;
        _questionRepo = questionRepo;
        _optionRepo = optionRepo;
        _sectionItemRepo = sectionItemRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<QuizResponseDto> GetQuizAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var quiz = await _context.Quizzes
            .AsNoTracking()
            .Include(q => q.Questions.OrderBy(qn => qn.Position))
                .ThenInclude(qn => qn.Options.OrderBy(o => o.Position))
            .Include(q => q.SectionItem)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Quiz not found.");

        return MapToDto(quiz);
    }

    public async Task<QuizResponseDto> CreateQuizAsync(Guid courseId, CreateQuizDto createDto, CancellationToken cancellationToken = default)
    {
        var section = await _context.Sections
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == createDto.SectionId && s.CourseId == courseId, cancellationToken)
            ?? throw new KeyNotFoundException("Section not found or does not belong to this course.");

        var quiz = new Quiz
        {
            Title = createDto.Title,
            Description = createDto.Description,
            DurationMinutes = createDto.DurationMinutes,
            PassingScorePercent = createDto.PassingScorePercent,
            MaxAttempts = createDto.MaxAttempts,
            ShuffleQuestions = createDto.ShuffleQuestions,
            ShuffleOptions = createDto.ShuffleOptions,
            ShowResultsImmediately = createDto.ShowResultsImmediately,
            AllowReview = createDto.AllowReview,
            AvailableFrom = createDto.AvailableFrom,
            AvailableUntil = createDto.AvailableUntil,
            CreatedAt = DateTime.UtcNow
        };

        await _quizRepo.AddAsync(quiz, cancellationToken);

        var position = await _context.SectionItems
            .CountAsync(si => si.SectionId == createDto.SectionId, cancellationToken) + 1;

        var sectionItem = new SectionItem
        {
            SectionId = createDto.SectionId,
            ItemType = SectionItemType.Quiz,
            ItemId = quiz.Id,
            Position = position,
            IsPreviewAllowed = false,
            IsMandatory = true
        };

        await _sectionItemRepo.AddAsync(sectionItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Quiz {QuizId} created in section {SectionId} (course {CourseId})", quiz.Id, createDto.SectionId, courseId);

        return await GetQuizAsync(quiz.Id, cancellationToken);
    }

    public async Task<QuizResponseDto> UpdateQuizAsync(Guid id, UpdateQuizDto updateDto, CancellationToken cancellationToken = default)
    {
        var quiz = await _quizRepo.FirstOrDefaultAsync(
            q => q.Id == id,
            include: qry => qry.Include(q => q.Questions),
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException("Quiz not found.");

        quiz.Title = updateDto.Title;
        quiz.Description = updateDto.Description;
        quiz.DurationMinutes = updateDto.DurationMinutes;
        quiz.PassingScorePercent = updateDto.PassingScorePercent;
        quiz.MaxAttempts = updateDto.MaxAttempts;
        quiz.ShuffleQuestions = updateDto.ShuffleQuestions;
        quiz.ShuffleOptions = updateDto.ShuffleOptions;
        quiz.ShowResultsImmediately = updateDto.ShowResultsImmediately;
        quiz.AllowReview = updateDto.AllowReview;
        quiz.AvailableFrom = updateDto.AvailableFrom;
        quiz.AvailableUntil = updateDto.AvailableUntil;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Quiz {QuizId} updated", id);

        return await GetQuizAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteQuizAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.SectionItem)
            .Include(q => q.Questions)
                .ThenInclude(qn => qn.Options)
            .Include(q => q.QuizAttempts)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (quiz is null)
            return false;

        if (quiz.QuizAttempts.Any(a => a.Status == QuizAttemptStatus.InProgress))
            throw new InvalidOperationException("Cannot delete quiz with in-progress attempts.");

        if (quiz.SectionItem is not null)
            await _sectionItemRepo.DeleteAsync(quiz.SectionItem, cancellationToken);

        foreach (var question in quiz.Questions)
            _context.Options.RemoveRange(question.Options);

        _context.Questions.RemoveRange(quiz.Questions);
        _context.Quizzes.Remove(quiz);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Quiz {QuizId} deleted", id);

        return true;
    }

    public async Task<QuestionResponseDto> AddQuestionAsync(Guid quizId, CreateQuestionDto createDto, CancellationToken cancellationToken = default)
    {
        var quizExists = await _quizRepo.AnyAsync(q => q.Id == quizId, cancellationToken);
        if (!quizExists)
            throw new KeyNotFoundException("Quiz not found.");

        var question = new Question
        {
            QuizId = quizId,
            QuestionText = createDto.QuestionText,
            Type = createDto.Type,
            Points = createDto.Points,
            Explanation = createDto.Explanation,
            Position = createDto.Position
        };

        await _questionRepo.AddAsync(question, cancellationToken);

        foreach (var optionDto in createDto.Options)
        {
            var option = new Option
            {
                QuestionId = question.Id,
                OptionText = optionDto.OptionText,
                IsCorrect = optionDto.IsCorrect,
                Position = optionDto.Position
            };
            await _optionRepo.AddAsync(option, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetQuestionAsync(question.Id, cancellationToken);
    }

    public async Task<QuestionResponseDto> UpdateQuestionAsync(Guid questionId, CreateQuestionDto updateDto, CancellationToken cancellationToken = default)
    {
        var question = await _context.Questions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken)
            ?? throw new KeyNotFoundException("Question not found.");

        question.QuestionText = updateDto.QuestionText;
        question.Type = updateDto.Type;
        question.Points = updateDto.Points;
        question.Explanation = updateDto.Explanation;
        question.Position = updateDto.Position;

        _context.Options.RemoveRange(question.Options);

        foreach (var optionDto in updateDto.Options)
        {
            var option = new Option
            {
                QuestionId = question.Id,
                OptionText = optionDto.OptionText,
                IsCorrect = optionDto.IsCorrect,
                Position = optionDto.Position
            };
            _context.Options.Add(option);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetQuestionAsync(question.Id, cancellationToken);
    }

    public async Task<bool> DeleteQuestionAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        var question = await _context.Questions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (question is null)
            return false;

        _context.Options.RemoveRange(question.Options);
        _context.Questions.Remove(question);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<QuestionResponseDto> GetQuestionAsync(Guid questionId, CancellationToken cancellationToken)
    {
        var question = await _context.Questions
            .AsNoTracking()
            .Include(q => q.Options.OrderBy(o => o.Position))
            .FirstAsync(q => q.Id == questionId, cancellationToken);

        return new QuestionResponseDto
        {
            Id = question.Id,
            QuestionText = question.QuestionText,
            Type = question.Type,
            Points = question.Points,
            Explanation = question.Explanation,
            Position = question.Position,
            Options = question.Options.Select(o => new OptionResponseDto
            {
                Id = o.Id,
                OptionText = o.OptionText,
                IsCorrect = o.IsCorrect,
                Position = o.Position
            }).ToList()
        };
    }

    private static QuizResponseDto MapToDto(Quiz quiz)
    {
        return new QuizResponseDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            DurationMinutes = quiz.DurationMinutes,
            PassingScorePercent = quiz.PassingScorePercent,
            MaxAttempts = quiz.MaxAttempts,
            ShuffleQuestions = quiz.ShuffleQuestions,
            ShuffleOptions = quiz.ShuffleOptions,
            ShowResultsImmediately = quiz.ShowResultsImmediately,
            AllowReview = quiz.AllowReview,
            TotalPoints = quiz.Questions.Sum(q => q.Points),
            QuestionCount = quiz.Questions.Count,
            CreatedAt = quiz.CreatedAt,
            Questions = quiz.Questions.Select(q => new QuestionResponseDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                Type = q.Type,
                Points = q.Points,
                Explanation = q.Explanation,
                Position = q.Position,
                Options = q.Options.Select(o => new OptionResponseDto
                {
                    Id = o.Id,
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect,
                    Position = o.Position
                }).ToList()
            }).ToList()
        };
    }
}
