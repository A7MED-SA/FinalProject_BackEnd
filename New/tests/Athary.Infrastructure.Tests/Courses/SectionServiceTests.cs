using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Athary.Domain.Entities;
using Athary.Infrastructure.Repositories;
using Athary.Infrastructure.Services.Courses;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Tests.Courses;

public class SectionServiceTests : SqliteTestBase
{
    private readonly ISectionService _sut;
    private readonly Course _course;

    public SectionServiceTests()
    {
        var user = new User { FirstName = "A", LastName = "B", Email = "a@b.com", UserName = "ab" };
        Context.Users.Add(user);
        var category = new Category { Name = "Cat", Slug = "cat" };
        Context.Categories.Add(category);
        Context.SaveChanges();

        _course = new Course { Id = Guid.NewGuid(), Title = "Test", Slug = "test-section", CategoryId = category.Id, CreatedBy = user.Id };
        Context.Courses.Add(_course);
        Context.SaveChanges();

        _sut = new SectionService(
            Context,
            new GenericRepository<Section>(Context),
            new GenericRepository<SectionItem>(Context),
            new UnitOfWork(Context));
    }

    [Fact]
    public async Task CreateSectionAsync_ShouldAddSectionAndReturnDto()
    {
        var dto = new CreateSectionDto { Title = "Section 1", Description = "Desc" };
        var result = await _sut.CreateSectionAsync(_course.Id, _course.CreatedBy, dto);

        result.Title.Should().Be("Section 1");
        var saved = await Context.Sections.FirstOrDefaultAsync(s => s.Id == result.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSectionsForCourseAsync_ShouldReturnOrderedByPosition()
    {
        Context.Sections.AddRange(
            new Section { CourseId = _course.Id, Title = "B", Position = 2 },
            new Section { CourseId = _course.Id, Title = "A", Position = 1 });
        await Context.SaveChangesAsync();

        var result = await _sut.GetSectionsForCourseAsync(_course.Id);

        result.Should().HaveCount(2);
        result[0].Title.Should().Be("A");
        result[1].Title.Should().Be("B");
    }
}
