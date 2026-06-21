using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Authentication;
using Athary.Application.Interfaces.Courses;
using Athary.Application.Interfaces.Media;
using Athary.Application.Interfaces.Notification;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Repositories;
using Athary.Infrastructure.Services.Courses;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Athary.Infrastructure.Tests.Courses;

public class CourseServiceTests : SqliteTestBase
{
    private readonly ICourseService _sut;
    private readonly User _instructor;
    private readonly Category _category;
    private readonly Mock<IObjectStorage> _objectStorageMock;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly Mock<IActivityLogService> _activityLogMock;

    public CourseServiceTests()
    {
        _instructor = new User { FirstName = "I", LastName = "Am", Email = "i@a.com", UserName = "instructor" };
        Context.Users.Add(_instructor);
        _category = new Category { Name = "Dev", Slug = "dev" };
        Context.Categories.Add(_category);
        Context.SaveChanges();

        _objectStorageMock = new Mock<IObjectStorage>();
        _objectStorageMock.Setup(s => s.GetPublicUrl(It.IsAny<string>(), It.IsAny<string>())).Returns("http://cdn.test/file");

        _notificationMock = new Mock<INotificationService>();
        _activityLogMock = new Mock<IActivityLogService>();

        _sut = new CourseService(
            Context,
            new GenericRepository<Course>(Context),
            new GenericRepository<CourseRequirement>(Context),
            new GenericRepository<CourseLearningOutcome>(Context),
            new GenericRepository<UploadedFile>(Context),
            new UnitOfWork(Context),
            _objectStorageMock.Object,
            _notificationMock.Object,
            _activityLogMock.Object,
            new Mock<ILogger<CourseService>>().Object);
    }

    [Fact]
    public async Task CreateCourseAsync_ShouldCreateAndReturnCourse()
    {
        var dto = new CreateCourseDto { Title = "My Course", Description = "Desc", CategoryId = _category.Id, Level = CourseLevel.Intermediate, Language = CourseLanguage.En, Price = 49.99m };

        var result = await _sut.CreateCourseAsync(_instructor.Id, dto);

        result.Title.Should().Be("My Course");
        result.Description.Should().Be("Desc");
        result.Status.Should().Be(CourseStatus.Draft);

        var saved = await Context.Courses.FirstOrDefaultAsync(c => c.Id == result.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCourseByIdAsync_ShouldReturnCourse_WhenExists()
    {
        var course = new Course { Id = Guid.NewGuid(), Title = "Existing", Slug = "existing", CategoryId = _category.Id, CreatedBy = _instructor.Id };
        Context.Courses.Add(course);
        await Context.SaveChangesAsync();

        var result = await _sut.GetCourseByIdAsync(course.Id);

        result.Title.Should().Be("Existing");
    }

    [Fact]
    public async Task GetCourseByIdAsync_ShouldThrow_WhenMissing()
    {
        await FluentActions.Awaiting(() => _sut.GetCourseByIdAsync(Guid.NewGuid()))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task AddRequirementAsync_ShouldAddRequirement()
    {
        var course = new Course { Id = Guid.NewGuid(), Title = "R", Slug = "r", CategoryId = _category.Id, CreatedBy = _instructor.Id };
        Context.Courses.Add(course);
        await Context.SaveChangesAsync();

        var result = await _sut.AddRequirementAsync(course.Id, _instructor.Id, new AddRequirementDto { RequirementText = "Basic knowledge" });

        result.RequirementText.Should().Be("Basic knowledge");
    }

    [Fact]
    public async Task SubmitForReviewAsync_ShouldChangeStatus()
    {
        var course = new Course { Id = Guid.NewGuid(), Title = "Submit Me", Slug = "submitme", Description = "Ready", CategoryId = _category.Id, CreatedBy = _instructor.Id };
        Context.Courses.Add(course);
        await Context.SaveChangesAsync();

        await _sut.SubmitForReviewAsync(course.Id, _instructor.Id);

        var updated = await Context.Courses.FindAsync(course.Id);
        updated!.Status.Should().Be(CourseStatus.PendingReview);
    }

    [Fact]
    public async Task ApproveCourseAsync_ShouldPublish()
    {
        var admin = new User { FirstName = "Ad", LastName = "Min", Email = "ad@m.com", UserName = "admin" };
        Context.Users.Add(admin);
        var course = new Course { Id = Guid.NewGuid(), Title = "Pub", Slug = "pub", Description = "Ready", CategoryId = _category.Id, CreatedBy = _instructor.Id, Status = CourseStatus.PendingReview };
        Context.Courses.Add(course);
        await Context.SaveChangesAsync();

        await _sut.ApproveCourseAsync(course.Id, admin.Id);

        var updated = await Context.Courses.FindAsync(course.Id);
        updated!.Status.Should().Be(CourseStatus.Published);
        updated.IsPublished.Should().BeTrue();
    }
}
