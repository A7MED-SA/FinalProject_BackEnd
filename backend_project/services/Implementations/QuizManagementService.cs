using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.Quiz;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class QuizManagementService : IQuizManagementService
{
    private readonly ApplicationDbContext _context;

    public QuizManagementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<QuizResponseDto> GetQuizAsync(Guid id)
    {
        var quiz = await _context.Quizzes
            .AsNoTracking()
            .Include(q => q.Questions.OrderBy(qn => qn.Position))
                .ThenInclude(qn => qn.Options.OrderBy(o => o.Position))
            .Include(q => q.SectionItem)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null)
            throw new KeyNotFoundException("Quiz not found.");

        return MapToDto(quiz);
    }

    public async Task<QuizResponseDto> CreateQuizAsync(CreateQuizDto createDto)
    {
        var section = await _context.Sections
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == createDto.SectionId);

        if (section == null)
            throw new KeyNotFoundException("Section not found.");

        var quiz = new Quiz
        {
            Id = Guid.NewGuid(),
            Title = createDto.Title,
            Description = createDto.Description,
            DurationMinutes = createDto.DurationMinutes,
            PassingScorePercent = (int)createDto.PassingScorePercent,
            MaxAttempts = createDto.MaxAttempts,
            ShuffleQuestions = createDto.ShuffleQuestions,
            ShuffleOptions = createDto.ShuffleOptions,
            ShowResultsImmediately = createDto.ShowResultsImmediately,
            AllowReview = createDto.AllowReview,
            CreatedAt = DateTime.UtcNow
        };

        _context.Quizzes.Add(quiz);

        var sectionItem = new SectionItem
        {
            Id = Guid.NewGuid(),
            SectionId = createDto.SectionId,
            ItemType = SectionItemType.Quiz,
            ItemId = quiz.Id,
            Position = await _context.SectionItems
                .Where(si => si.SectionId == createDto.SectionId)
                .CountAsync() + 1,
            IsPreviewAllowed = false,
            IsMandatory = true
        };

        _context.SectionItems.Add(sectionItem);
        await _context.SaveChangesAsync();

        return await GetQuizAsync(quiz.Id);
    }

    public async Task<QuizResponseDto> UpdateQuizAsync(Guid id, CreateQuizDto updateDto)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null)
            throw new KeyNotFoundException("Quiz not found.");

        quiz.Title = updateDto.Title;
        quiz.Description = updateDto.Description;
        quiz.DurationMinutes = updateDto.DurationMinutes;
        quiz.PassingScorePercent = (int)updateDto.PassingScorePercent;
        quiz.MaxAttempts = updateDto.MaxAttempts;
        quiz.ShuffleQuestions = updateDto.ShuffleQuestions;
        quiz.ShuffleOptions = updateDto.ShuffleOptions;
        quiz.ShowResultsImmediately = updateDto.ShowResultsImmediately;
        quiz.AllowReview = updateDto.AllowReview;

        await _context.SaveChangesAsync();

        return await GetQuizAsync(quiz.Id);
    }

    public async Task<bool> DeleteQuizAsync(Guid id)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.SectionItem)
            .Include(q => q.Questions)
                .ThenInclude(qn => qn.Options)
            .Include(q => q.QuizAttempts)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null)
            return false;

        if (quiz.QuizAttempts.Any(a => a.Status == QuizAttemptStatus.InProgress))
            throw new InvalidOperationException("Cannot delete quiz with in-progress attempts.");

        if (quiz.SectionItem != null)
            _context.SectionItems.Remove(quiz.SectionItem);

        foreach (var question in quiz.Questions)
        {
            _context.Options.RemoveRange(question.Options);
        }
        _context.Questions.RemoveRange(quiz.Questions);
        _context.Quizzes.Remove(quiz);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<QuestionResponseDto> AddQuestionAsync(Guid quizId, CreateQuestionDto createDto)
    {
        var quiz = await _context.Quizzes
            .AnyAsync(q => q.Id == quizId);

        if (!quiz)
            throw new KeyNotFoundException("Quiz not found.");

        var question = new Question
        {
            Id = Guid.NewGuid(),
            QuizId = quizId,
            QuestionText = createDto.QuestionText,
            Type = createDto.Type,
            Points = createDto.Points,
            Explanation = createDto.Explanation,
            Position = createDto.OrderIndex
        };

        _context.Questions.Add(question);

        foreach (var optionDto in createDto.Options)
        {
            var option = new Option
            {
                Id = Guid.NewGuid(),
                QuestionId = question.Id,
                OptionText = optionDto.OptionText,
                IsCorrect = optionDto.IsCorrect,
                Position = optionDto.OrderIndex
            };
            _context.Options.Add(option);
        }

        await _context.SaveChangesAsync();

        return await GetQuestionAsync(question.Id);
    }

    public async Task<QuestionResponseDto> UpdateQuestionAsync(Guid questionId, CreateQuestionDto updateDto)
    {
        var question = await _context.Questions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == questionId);

        if (question == null)
            throw new KeyNotFoundException("Question not found.");

        question.QuestionText = updateDto.QuestionText;
        question.Type = updateDto.Type;
        question.Points = updateDto.Points;
        question.Explanation = updateDto.Explanation;
        question.Position = updateDto.OrderIndex;

        _context.Options.RemoveRange(question.Options);

        foreach (var optionDto in updateDto.Options)
        {
            var option = new Option
            {
                Id = Guid.NewGuid(),
                QuestionId = question.Id,
                OptionText = optionDto.OptionText,
                IsCorrect = optionDto.IsCorrect,
                Position = optionDto.OrderIndex
            };
            _context.Options.Add(option);
        }

        await _context.SaveChangesAsync();

        return await GetQuestionAsync(question.Id);
    }

    public async Task<bool> DeleteQuestionAsync(Guid questionId)
    {
        var question = await _context.Questions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == questionId);

        if (question == null)
            return false;

        _context.Options.RemoveRange(question.Options);
        _context.Questions.Remove(question);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<QuestionResponseDto> GetQuestionAsync(Guid questionId)
    {
        var question = await _context.Questions
            .AsNoTracking()
            .Include(q => q.Options.OrderBy(o => o.Position))
            .FirstAsync(q => q.Id == questionId);

        return new QuestionResponseDto
        {
            Id = question.Id,
            QuestionText = question.QuestionText,
            Type = question.Type,
            Points = question.Points,
            Explanation = question.Explanation,
            OrderIndex = question.Position,
            Options = question.Options.Select(o => new OptionResponseDto
            {
                Id = o.Id,
                OptionText = o.OptionText,
                IsCorrect = o.IsCorrect,
                OrderIndex = o.Position
            })
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
                OrderIndex = q.Position,
                Options = q.Options.Select(o => new OptionResponseDto
                {
                    Id = o.Id,
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect,
                    OrderIndex = o.Position
                })
            })
        };
    }
}
