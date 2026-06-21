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

public class VideoServiceTests : SqliteTestBase
{
    private readonly IVideoService _sut;
    private readonly Course _course;
    private readonly Section _section;
    private readonly UploadedFile _videoFile;

    public VideoServiceTests()
    {
        var user = new User { FirstName = "A", LastName = "B", Email = "a@b.com", UserName = "ab" };
        Context.Users.Add(user);
        var category = new Category { Name = "Cat", Slug = "cat" };
        Context.Categories.Add(category);
        Context.SaveChanges();

        _course = new Course { Id = Guid.NewGuid(), Title = "C1", Slug = "c1-video", CategoryId = category.Id, CreatedBy = user.Id };
        Context.Courses.Add(_course);

        _section = new Section { Id = Guid.NewGuid(), CourseId = _course.Id, Title = "S1" };
        Context.Sections.Add(_section);

        _videoFile = new UploadedFile { Id = Guid.NewGuid(), FilePath = "/v/test.mp4", FileName = "test.mp4", OriginalName = "test.mp4", FileType = StoredFileType.Video, Bucket = "videos", UploadedBy = user.Id };
        Context.Files.Add(_videoFile);
        Context.SaveChanges();

        _sut = new VideoService(
            Context,
            new GenericRepository<Video>(Context),
            new GenericRepository<SectionItem>(Context),
            new UnitOfWork(Context),
            new Mock<IObjectStorage>().Object,
            new Mock<ILogger<VideoService>>().Object);
    }

    [Fact]
    public async Task CreateVideoAsync_ShouldCreateVideoAndSectionItem()
    {
        var dto = new CreateVideoDto
        {
            SectionId = _section.Id,
            Title = "Lesson 1",
            VideoFileId = _videoFile.Id,
            DurationSeconds = 300
        };

        var result = await _sut.CreateVideoAsync(_course.Id, dto);

        result.Title.Should().Be("Lesson 1");
        var video = await Context.Videos.FirstOrDefaultAsync(v => v.Id == result.Id);
        video.Should().NotBeNull();
        var sectionItem = await Context.SectionItems
            .FirstOrDefaultAsync(si => si.ItemId == result.Id && si.ItemType == SectionItemType.Video);
        sectionItem.Should().NotBeNull();
    }
}
