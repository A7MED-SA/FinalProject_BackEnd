using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Athary.Application.Interfaces.Media;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Services.Courses;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Athary.Infrastructure.Tests.Courses;

public class PublicCourseServiceTests : SqliteTestBase
{
    private readonly IPublicCourseService _sut;
    private readonly User _instructor;
    private readonly Category _category;
    private readonly Course _publishedCourse;

    public PublicCourseServiceTests()
    {
        _instructor = new User { FirstName = "I", LastName = "Am", Email = "i@a.com", UserName = "instructor" };
        Context.Users.Add(_instructor);
        _category = new Category { Name = "Dev", Slug = "dev" };
        Context.Categories.Add(_category);
        Context.SaveChanges();

        _publishedCourse = new Course
        {
            Id = Guid.NewGuid(), Title = "Public Course", Slug = "public-course",
            Description = "Learn things", CategoryId = _category.Id, CreatedBy = _instructor.Id,
            Status = CourseStatus.Published, IsPublished = true, Price = 0, PublishedAt = DateTime.UtcNow,
            Level = CourseLevel.Beginner, Language = CourseLanguage.En
        };
        Context.Courses.Add(_publishedCourse);
        Context.SaveChanges();

        var objectStorageMock = new Mock<IObjectStorage>();
        objectStorageMock.Setup(s => s.GetPublicUrl(It.IsAny<string>(), It.IsAny<string>())).Returns("http://cdn.test/file");

        _sut = new PublicCourseService(
            Context,
            objectStorageMock.Object,
            new Mock<ILogger<PublicCourseService>>().Object);
    }

    [Fact]
    public async Task GetPublishedCoursesAsync_ShouldReturnPublishedCourses()
    {
        var result = await _sut.GetPublishedCoursesAsync(new PublicCourseFilterDto());

        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("Public Course");
    }

    [Fact]
    public async Task GetPublishedCoursesAsync_ShouldFilterByCategory()
    {
        var filter = new PublicCourseFilterDto { CategoryId = _category.Id };

        var result = await _sut.GetPublishedCoursesAsync(filter);

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPublishedCoursesAsync_ShouldExcludeUnpublished()
    {
        var draft = new Course { Id = Guid.NewGuid(), Title = "Draft", Slug = "draft", CategoryId = _category.Id, CreatedBy = _instructor.Id, Status = CourseStatus.Draft };
        Context.Courses.Add(draft);
        await Context.SaveChangesAsync();

        var result = await _sut.GetPublishedCoursesAsync(new PublicCourseFilterDto());

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCourseDetailsAsync_ShouldReturnDetails()
    {
        var result = await _sut.GetCourseDetailsAsync(_publishedCourse.Id);

        result.Title.Should().Be("Public Course");
        result.Instructor.FullName.Should().Be("I Am");
    }

    [Fact]
    public async Task GetCourseDetailsBySlugAsync_ShouldReturnCourse()
    {
        var result = await _sut.GetCourseDetailsBySlugAsync("public-course");

        result.Title.Should().Be("Public Course");
    }

    [Fact]
    public async Task SearchCoursesSuggestAsync_ShouldReturnMatches()
    {
        var results = await _sut.SearchCoursesSuggestAsync("Public");

        results.Should().Contain(r => r.Title == "Public Course");
    }

    [Fact]
    public async Task SearchCoursesSuggestAsync_ShortQuery_ShouldReturnEmpty()
    {
        var results = await _sut.SearchCoursesSuggestAsync("x");

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPlatformStatsAsync_ShouldReturnStats()
    {
        var stats = await _sut.GetPlatformStatsAsync();

        stats.TotalCourses.Should().Be(1);
    }
}
