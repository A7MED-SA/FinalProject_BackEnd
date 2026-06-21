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

public class ContentProgressServiceTests : SqliteTestBase
{
    private readonly IContentProgressService _sut;
    private readonly Enrollment _enrollment;
    private readonly Guid _sectionItemId;

    public ContentProgressServiceTests()
    {
        var user = new User { FirstName = "A", LastName = "B", Email = "a@b.com", UserName = "ab" };
        Context.Users.Add(user);
        var category = new Category { Name = "Cat", Slug = "cat" };
        Context.Categories.Add(category);
        Context.SaveChanges();

        var course = new Course { Id = Guid.NewGuid(), Title = "C1", Slug = "c1-progress", CategoryId = category.Id, CreatedBy = user.Id };
        var section = new Section { Id = Guid.NewGuid(), CourseId = course.Id, Title = "S1" };
        _enrollment = new Enrollment { Id = Guid.NewGuid(), UserId = user.Id, CourseId = course.Id, Status = EnrollmentStatus.InProgress };

        Context.Courses.Add(course);
        Context.Sections.Add(section);
        Context.Enrollments.Add(_enrollment);
        _sectionItemId = Guid.NewGuid();
        Context.SectionItems.AddRange(
            new SectionItem { Id = Guid.NewGuid(), SectionId = section.Id, ItemType = SectionItemType.Video, ItemId = _sectionItemId },
            new SectionItem { Id = Guid.NewGuid(), SectionId = section.Id, ItemType = SectionItemType.Quiz, ItemId = Guid.NewGuid() });
        Context.SaveChanges();

        _sut = new ContentProgressService(
            Context,
            new GenericRepository<ContentProgress>(Context),
            new GenericRepository<Enrollment>(Context),
            new UnitOfWork(Context),
            new Mock<ILogger<ContentProgressService>>().Object);
    }

    [Fact]
    public async Task MarkCompletedAsync_ShouldCreateProgressRecord()
    {
        var result = await _sut.MarkCompletedAsync(_enrollment.Id, _sectionItemId, ContentType.Video);

        result.IsCompleted.Should().BeTrue();
        var saved = await Context.ContentProgresses
            .FirstOrDefaultAsync(cp => cp.EnrollmentId == _enrollment.Id && cp.ContentId == _sectionItemId);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProgressForEnrollmentAsync_ShouldReturnProgressRecords()
    {
        await _sut.MarkCompletedAsync(_enrollment.Id, _sectionItemId, ContentType.Video);

        var progress = await _sut.GetProgressForEnrollmentAsync(_enrollment.Id);
        progress.Should().Contain(p => p.ContentId == _sectionItemId && p.IsCompleted);
    }

    [Fact]
    public async Task RecalculateEnrollmentProgressAsync_ShouldUpdateEnrollment()
    {
        await _sut.MarkCompletedAsync(_enrollment.Id, _sectionItemId, ContentType.Video);

        var enrollment = await Context.Enrollments.FindAsync(_enrollment.Id);
        enrollment!.ProgressPercentage.Should().Be(50);
    }
}
