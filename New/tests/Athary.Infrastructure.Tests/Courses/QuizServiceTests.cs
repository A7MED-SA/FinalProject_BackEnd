using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Repositories;
using Athary.Infrastructure.Services.Courses;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Athary.Infrastructure.Tests.Courses;

public class QuizServiceTests : SqliteTestBase
{
    private readonly IQuizService _sut;
    private readonly Course _course;
    private readonly Section _section;

    public QuizServiceTests()
    {
        var user = new User { FirstName = "A", LastName = "B", Email = "a@b.com", UserName = "ab" };
        Context.Users.Add(user);
        var category = new Category { Name = "Cat", Slug = "cat" };
        Context.Categories.Add(category);
        Context.SaveChanges();

        _course = new Course { Id = Guid.NewGuid(), Title = "C1", Slug = "c1-quiz", CategoryId = category.Id, CreatedBy = user.Id };
        Context.Courses.Add(_course);

        _section = new Section { Id = Guid.NewGuid(), CourseId = _course.Id, Title = "S1" };
        Context.Sections.Add(_section);
        Context.SaveChanges();

        _sut = new QuizService(
            Context,
            new GenericRepository<Quiz>(Context),
            new GenericRepository<Question>(Context),
            new GenericRepository<Option>(Context),
            new GenericRepository<SectionItem>(Context),
            new UnitOfWork(Context),
            new Mock<ILogger<QuizService>>().Object);
    }

    [Fact]
    public async Task CreateQuizAsync_ShouldCreateQuizAndSectionItem()
    {
        var dto = new CreateQuizDto
        {
            SectionId = _section.Id, Title = "Quiz 1", Description = "Test quiz",
            PassingScorePercent = 70, MaxAttempts = 3
        };

        var result = await _sut.CreateQuizAsync(_course.Id, dto);

        result.Title.Should().Be("Quiz 1");
        var si = await Context.SectionItems
            .FirstOrDefaultAsync(s => s.ItemType == SectionItemType.Quiz && s.ItemId == result.Id);
        si.Should().NotBeNull();
    }

    [Fact]
    public async Task GetQuizAsync_ShouldReturnQuizWithQuestions()
    {
        var quiz = new Quiz
        {
            Id = Guid.NewGuid(), Title = "Q1",
            Questions = new List<Question> { new() { Id = Guid.NewGuid(), QuestionText = "What?", Points = 10, Position = 1 } }
        };
        Context.Quizzes.Add(quiz);
        await Context.SaveChangesAsync();

        var result = await _sut.GetQuizAsync(quiz.Id);

        result.Title.Should().Be("Q1");
        result.Questions.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddQuestionAsync_ShouldAddQuestionWithOptions()
    {
        var quiz = new Quiz { Id = Guid.NewGuid(), Title = "Q1" };
        Context.Quizzes.Add(quiz);
        await Context.SaveChangesAsync();

        var dto = new CreateQuestionDto
        {
            QuestionText = "What is 2+2?", Points = 10,
            Options = new List<CreateOptionDto>
            {
                new() { OptionText = "4", IsCorrect = true, Position = 0 },
                new() { OptionText = "22", IsCorrect = false, Position = 1 }
            }
        };

        var result = await _sut.AddQuestionAsync(quiz.Id, dto);

        result.Options.Should().HaveCount(2);
        result.Options.Should().Contain(o => o.OptionText == "4" && o.IsCorrect == true);
    }

    [Fact]
    public async Task UpdateQuestionAsync_ShouldUpdateQuestion()
    {
        var question = new Question { Id = Guid.NewGuid(), QuestionText = "Old text", Points = 5, QuizId = Guid.NewGuid() };
        Context.Questions.Add(question);
        await Context.SaveChangesAsync();

        var dto = new CreateQuestionDto
        {
            QuestionText = "New text", Points = 10,
            Options = new List<CreateOptionDto> { new() { OptionText = "A", IsCorrect = true, Position = 0 } }
        };

        var result = await _sut.UpdateQuestionAsync(question.Id, dto);

        result.QuestionText.Should().Be("New text");
    }
}
