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

public class QuizAttemptServiceTests : SqliteTestBase
{
    private readonly IQuizAttemptService _sut;
    private readonly Enrollment _enrollment;
    private readonly Quiz _quiz;
    private readonly Mock<IContentProgressService> _contentProgressMock;

    public QuizAttemptServiceTests()
    {
        _contentProgressMock = new Mock<IContentProgressService>();

        var user = new User { FirstName = "A", LastName = "B", Email = "a@b.com", UserName = "ab" };
        Context.Users.Add(user);
        var category = new Category { Name = "Cat", Slug = "cat" };
        Context.Categories.Add(category);
        Context.SaveChanges();

        var course = new Course { Id = Guid.NewGuid(), Title = "C1", Slug = "c1-quizatt", CategoryId = category.Id, CreatedBy = user.Id, Status = CourseStatus.Published, IsPublished = true };
        Context.Courses.Add(course);

        var section = new Section { Id = Guid.NewGuid(), CourseId = course.Id, Title = "S1" };
        Context.Sections.Add(section);

        _enrollment = new Enrollment { Id = Guid.NewGuid(), UserId = user.Id, CourseId = course.Id, Status = EnrollmentStatus.InProgress };
        Context.Enrollments.Add(_enrollment);

        var questionId = Guid.NewGuid();
        _quiz = new Quiz
        {
            Id = Guid.NewGuid(),
            Title = "Q1",
            PassingScorePercent = 50,
            MaxAttempts = 3,
            Questions = new List<Question>
            {
                new()
                {
                    Id = questionId, QuestionText = "What is 2+2?", Points = 10, Position = 1,
                    Options = new List<Option>
                    {
                        new() { Id = Guid.NewGuid(), OptionText = "4", IsCorrect = true, Position = 0 },
                        new() { Id = Guid.NewGuid(), OptionText = "22", IsCorrect = false, Position = 1 }
                    }
                }
            }
        };
        Context.Quizzes.Add(_quiz);
        Context.SaveChanges();

        _sut = new QuizAttemptService(
            Context,
            new GenericRepository<QuizAttempt>(Context),
            new GenericRepository<UserAnswer>(Context),
            new UnitOfWork(Context),
            _contentProgressMock.Object,
            new Mock<ILogger<QuizAttemptService>>().Object);
    }

    [Fact]
    public async Task StartAttemptAsync_ShouldCreateAttempt()
    {
        var result = await _sut.StartAttemptAsync(_quiz.Id, _enrollment.Id);

        result.QuizId.Should().Be(_quiz.Id);
        result.EnrollmentId.Should().Be(_enrollment.Id);
        result.Status.Should().Be(QuizAttemptStatus.InProgress.ToString());
    }

    [Fact]
    public async Task SubmitAttemptAsync_ShouldGradeAndReturnResult()
    {
        var attempt = await _sut.StartAttemptAsync(_quiz.Id, _enrollment.Id);
        var correctOptionId = _quiz.Questions[0].Options[0].Id;

        var result = await _sut.SubmitAttemptAsync(attempt.Id, new SubmitAttemptDto
        {
            Answers = new List<SubmitAnswerDto>
            {
                new() { QuestionId = _quiz.Questions[0].Id, SelectedOptionId = correctOptionId }
            }
        });

        result.IsPassed.Should().BeTrue();
        result.ScorePercentage.Should().Be(100);
    }

    [Fact]
    public async Task GetUserAttemptsForQuizAsync_ShouldReturnOrderedAttempts()
    {
        var attempt = await _sut.StartAttemptAsync(_quiz.Id, _enrollment.Id);

        var attempts = await _sut.GetUserAttemptsForQuizAsync(_quiz.Id, _enrollment.Id);

        attempts.Should().Contain(a => a.Id == attempt.Id);
    }
}
