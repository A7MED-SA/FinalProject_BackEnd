using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Authentication;
using Athary.Application.Interfaces.Courses;
using Athary.Application.Interfaces.Notification;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Services.Courses;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Athary.Infrastructure.Tests.Courses;

public class CourseEditApprovalServiceTests : SqliteTestBase
{
    private readonly ICourseEditApprovalService _sut;
    private readonly Course _course;
    private readonly User _instructor;
    private readonly User _admin;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly Mock<IEmailService> _emailMock;
    private readonly Mock<IActivityLogService> _activityLogMock;

    public CourseEditApprovalServiceTests()
    {
        _instructor = new User { FirstName = "I", LastName = "Am", Email = "i@a.com", UserName = "instructor" };
        Context.Users.Add(_instructor);
        _admin = new User { FirstName = "Ad", LastName = "Min", Email = "ad@m.com", UserName = "admin" };
        Context.Users.Add(_admin);
        var category = new Category { Name = "Cat", Slug = "cat" };
        Context.Categories.Add(category);
        Context.SaveChanges();

        _course = new Course { Id = Guid.NewGuid(), Title = "Test Course", Slug = "test-course-edit", CategoryId = category.Id, CreatedBy = _instructor.Id, Status = CourseStatus.Published, IsPublished = true };
        Context.Courses.Add(_course);
        Context.SaveChanges();

        _notificationMock = new Mock<INotificationService>();
        _emailMock = new Mock<IEmailService>();
        _activityLogMock = new Mock<IActivityLogService>();

        _sut = new CourseEditApprovalService(
            Context,
            _notificationMock.Object,
            _emailMock.Object,
            _activityLogMock.Object,
            new Mock<ILogger<CourseEditApprovalService>>().Object);
    }

    [Fact]
    public async Task RequestEditAsync_NonSensitiveProperty_ShouldApplyDirectly()
    {
        var result = await _sut.RequestEditAsync(
            _course.Id, _instructor.Id,
            EditRequestType.CourseProperty, EditOperation.Update,
            "Description", null, "New description");

        result.AppliedImmediately.Should().BeTrue();
        result.Message.Should().Be("Edit applied successfully");

        var updated = await Context.Courses.FindAsync(_course.Id);
        updated!.Description.Should().Be("New description");
    }

    [Fact]
    public async Task RequestEditAsync_SectionEdit_ShouldCreatePendingRequest()
    {
        var section = new Section { Id = Guid.NewGuid(), CourseId = _course.Id, Title = "S1" };
        Context.Sections.Add(section);
        await Context.SaveChangesAsync();

        var result = await _sut.RequestEditAsync(
            _course.Id, _instructor.Id,
            EditRequestType.Section, EditOperation.Update,
            null, section.Id,
            """{"title":"Updated Section"}""");

        result.AppliedImmediately.Should().BeFalse();
        result.Status.Should().Be(EditRequestStatus.Pending);

        var request = await Context.CourseEditRequests.FirstOrDefaultAsync();
        request.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPendingRequestsAsync_ShouldReturnPending()
    {
        Context.CourseEditRequests.Add(new CourseEditRequest
        {
            CourseId = _course.Id,
            RequestedBy = _instructor.Id,
            RequestType = EditRequestType.CourseProperty,
            Operation = EditOperation.Update,
            JsonPayload = """{"title":"New Title"}""",
            Status = EditRequestStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
        await Context.SaveChangesAsync();

        var result = await _sut.GetPendingRequestsAsync(new EditRequestFilterDto());

        result.Items.Should().HaveCount(1);
        result.Items[0].RequestType.Should().Be(EditRequestType.CourseProperty);
    }

    [Fact]
    public async Task ReviewRequestAsync_Approve_ShouldApplyChanges()
    {
        var section = new Section { Id = Guid.NewGuid(), CourseId = _course.Id, Title = "Original" };
        Context.Sections.Add(section);
        await Context.SaveChangesAsync();

        var request = new CourseEditRequest
        {
            CourseId = _course.Id,
            RequestedBy = _instructor.Id,
            RequestType = EditRequestType.Section,
            Operation = EditOperation.Update,
            TargetSectionId = section.Id,
            JsonPayload = """{"title":"Updated Section"}""",
            Status = EditRequestStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        Context.CourseEditRequests.Add(request);
        await Context.SaveChangesAsync();

        var result = await _sut.ReviewRequestAsync(request.Id, _admin.Id, true, "Looks good");

        result.Status.Should().Be(EditRequestStatus.Approved);

        var updated = await Context.Sections.FindAsync(section.Id);
        updated!.Title.Should().Be("Updated Section");
    }
}
