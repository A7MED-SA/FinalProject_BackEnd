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

public class EnrollmentServiceTests : SqliteTestBase
{
    private readonly IEnrollmentService _sut;
    private readonly User _user;
    private readonly Course _course;

    public EnrollmentServiceTests()
    {
        _user = new User { FirstName = "A", LastName = "B", Email = "a@b.com", UserName = "ab" };
        Context.Users.Add(_user);
        var category = new Category { Name = "Cat", Slug = "cat" };
        Context.Categories.Add(category);
        Context.SaveChanges();

        _course = new Course
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Slug = "test-enroll",
            CategoryId = category.Id,
            CreatedBy = _user.Id,
            Status = CourseStatus.Published,
            IsPublished = true
        };
        Context.Courses.Add(_course);
        Context.SaveChanges();

        _sut = new EnrollmentService(
            Context,
            new GenericRepository<Enrollment>(Context),
            new GenericRepository<Course>(Context),
            new UnitOfWork(Context),
            new Mock<ILogger<EnrollmentService>>().Object);
    }

    [Fact]
    public async Task EnrollUserAsync_ShouldCreateEnrollment_WhenCoursePublished()
    {
        var dto = new CreateEnrollmentDto { CourseId = _course.Id, UserId = _user.Id };
        var result = await _sut.EnrollUserAsync(dto);

        result.CourseId.Should().Be(_course.Id);
        result.UserId.Should().Be(_user.Id);
        result.Status.Should().Be(EnrollmentStatus.InProgress);

        var saved = await Context.Enrollments.FirstOrDefaultAsync(e => e.UserId == _user.Id && e.CourseId == _course.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserEnrollmentsAsync_ShouldReturnUserEnrollments()
    {
        Context.Enrollments.Add(new Enrollment { UserId = _user.Id, CourseId = _course.Id, Status = EnrollmentStatus.InProgress });
        await Context.SaveChangesAsync();

        var result = await _sut.GetUserEnrollmentsAsync(_user.Id);

        result.Should().HaveCount(1);
    }
}
