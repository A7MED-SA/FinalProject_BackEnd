using Athary.Application.DTOs.Communication;
using Athary.Application.Interfaces.Authentication;
using Athary.Application.Interfaces.Notification;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Services.Communication;
using FluentAssertions;
using Mapster;
using MapsterMapper;
using Moq;

namespace Athary.Infrastructure.Tests.Communication;

public sealed class ReportServiceTests : SqliteTestBase
{
    private readonly ReportService _sut;
    private readonly User _reporter;
    private readonly User _admin;
    private readonly Course _course;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly Mock<IActivityLogService> _activityLogMock;

    public ReportServiceTests()
    {
        _reporter = new User { FirstName = "Rep", LastName = "R", Email = "r@r.com", UserName = "rr" };
        _admin = new User { FirstName = "Adm", LastName = "N", Email = "n@n.com", UserName = "nn" };
        Context.Users.AddRange(_reporter, _admin);

        var cat = new Category { Name = "Cat", Slug = "cat" };
        Context.Categories.Add(cat);

        _course = new Course
        {
            Title = "Bad Course", Slug = "bad-course", Price = 10,
            CategoryId = cat.Id, CreatedBy = _admin.Id,
            Status = CourseStatus.Published, IsPublished = true
        };
        Context.Courses.Add(_course);
        Context.SaveChanges();

        _notificationMock = new Mock<INotificationService>();
        _activityLogMock = new Mock<IActivityLogService>();

        _sut = new ReportService(Context, _notificationMock.Object, _activityLogMock.Object, Mapper);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateNewReport()
    {
        var result = await _sut.CreateAsync(_reporter.Id, new CreateReportRequest
        {
            EntityType = "Course",
            EntityId = _course.Id,
            Reason = "Spam",
            Description = "This course is spam"
        });

        result.EntityType.Should().Be("Course");
        result.EntityId.Should().Be(_course.Id);
        result.Reason.Should().Be("Spam");
        result.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task CreateAsync_ShouldUpdateExistingPendingReport()
    {
        var first = await _sut.CreateAsync(_reporter.Id, new CreateReportRequest
        {
            EntityType = "Course",
            EntityId = _course.Id,
            Reason = "Spam",
            Description = "Original description"
        });

        var second = await _sut.CreateAsync(_reporter.Id, new CreateReportRequest
        {
            EntityType = "Course",
            EntityId = _course.Id,
            Reason = "Spam",
            Description = "Updated description"
        });

        first.Id.Should().Be(second.Id);
        second.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenInvalidEntityType()
    {
        await FluentActions.Invoking(() => _sut.CreateAsync(_reporter.Id, new CreateReportRequest
        {
            EntityType = "InvalidType",
            EntityId = Guid.NewGuid(),
            Reason = "Other"
        })).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetPendingAsync_ShouldReturnPendingReports()
    {
        await _sut.CreateAsync(_reporter.Id, new CreateReportRequest
        {
            EntityType = "Course", EntityId = _course.Id, Reason = "Inappropriate"
        });

        var result = await _sut.GetPendingAsync();

        result.Items.Should().HaveCount(1);
        result.Items[0].Reason.Should().Be("Inappropriate");
    }

    [Fact]
    public async Task ResolveAsync_ShouldMarkAsActionTaken()
    {
        var created = await _sut.CreateAsync(_reporter.Id, new CreateReportRequest
        {
            EntityType = "Course", EntityId = _course.Id, Reason = "Copyright"
        });

        var resolved = await _sut.ResolveAsync(created.Id, new ResolveReportRequest
        {
            Status = "ActionTaken",
            AdminNote = "Content removed"
        }, _admin.Id);

        resolved.Status.Should().Be("ActionTaken");
        resolved.AdminNote.Should().Be("Content removed");
        _notificationMock.Verify(n => n.CreateNotificationAsync(
            _reporter.Id, It.IsAny<string>(), It.IsAny<string>(),
            NotificationType.System, null, null, It.IsAny<CancellationToken>()), Times.Once);
        _activityLogMock.Verify(a => a.LogActivityAsync(
            _admin.Id, "ReportResolved", It.IsAny<string>(),
            "system", null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResolveAsync_ShouldThrow_WhenReportNotFound()
    {
        await FluentActions.Invoking(() => _sut.ResolveAsync(Guid.NewGuid(), new ResolveReportRequest
        {
            Status = "Dismissed"
        }, _admin.Id)).Should().ThrowAsync<KeyNotFoundException>();
    }
}
