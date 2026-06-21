using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Athary.Application.Interfaces.Media;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Repositories;
using Athary.Infrastructure.Services.Courses;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Athary.Infrastructure.Tests.Courses;

public class DocumentServiceTests : SqliteTestBase
{
    private readonly IDocumentService _sut;
    private readonly Course _course;
    private readonly Section _section;
    private readonly UploadedFile _file;

    public DocumentServiceTests()
    {
        var user = new User { FirstName = "A", LastName = "B", Email = "a@b.com", UserName = "ab" };
        Context.Users.Add(user);
        var category = new Category { Name = "Cat", Slug = "cat" };
        Context.Categories.Add(category);
        Context.SaveChanges();

        _course = new Course { Id = Guid.NewGuid(), Title = "C1", Slug = "c1-doc", CategoryId = category.Id, CreatedBy = user.Id };
        Context.Courses.Add(_course);

        _section = new Section { Id = Guid.NewGuid(), CourseId = _course.Id, Title = "S1" };
        Context.Sections.Add(_section);

        _file = new UploadedFile { Id = Guid.NewGuid(), FilePath = "/d/doc.pdf", FileName = "doc.pdf", OriginalName = "doc.pdf", FileType = StoredFileType.Document, Bucket = "docs", UploadedBy = user.Id };
        Context.Files.Add(_file);
        Context.SaveChanges();

        _sut = new DocumentService(
            Context,
            new GenericRepository<Document>(Context),
            new GenericRepository<SectionItem>(Context),
            new UnitOfWork(Context),
            new Mock<IObjectStorage>().Object,
            new Mock<ILogger<DocumentService>>().Object);
    }

    [Fact]
    public async Task CreateDocumentAsync_ShouldCreateDocumentAndSectionItem()
    {
        var dto = new CreateDocumentDto { SectionId = _section.Id, Title = "Doc 1", FileId = _file.Id };
        var result = await _sut.CreateDocumentAsync(_course.Id, dto);

        result.Title.Should().Be("Doc 1");
        var doc = await Context.Documents.FirstOrDefaultAsync(d => d.Id == result.Id);
        doc.Should().NotBeNull();
        var sectionItem = await Context.SectionItems
            .FirstOrDefaultAsync(si => si.ItemId == result.Id && si.ItemType == SectionItemType.Document);
        sectionItem.Should().NotBeNull();
    }
}
