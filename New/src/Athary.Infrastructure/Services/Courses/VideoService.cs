using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Athary.Application.Interfaces.Media;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Domain.Interfaces;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Athary.Infrastructure.Services.Courses;

public sealed class VideoService : IVideoService
{
    private readonly ApplicationDbContext _context;
    private readonly IRepository<Video> _videoRepo;
    private readonly IRepository<SectionItem> _sectionItemRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IObjectStorage _objectStorage;
    private readonly ILogger<VideoService> _logger;

    public VideoService(
        ApplicationDbContext context,
        IRepository<Video> videoRepo,
        IRepository<SectionItem> sectionItemRepo,
        IUnitOfWork unitOfWork,
        IObjectStorage objectStorage,
        ILogger<VideoService> logger)
    {
        _context = context;
        _videoRepo = videoRepo;
        _sectionItemRepo = sectionItemRepo;
        _unitOfWork = unitOfWork;
        _objectStorage = objectStorage;
        _logger = logger;
    }

    public async Task<VideoResponseDto> GetVideoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var video = await _videoRepo.FirstOrDefaultAsync(
            v => v.Id == id,
            include: q => q.Include(v => v.SectionItem!).ThenInclude(si => si.Section),
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException("Video not found.");

        return MapToDto(video);
    }

    public async Task<VideoResponseDto> CreateVideoAsync(Guid courseId, CreateVideoDto createDto, CancellationToken cancellationToken = default)
    {
        var section = await _context.Sections
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == createDto.SectionId && s.CourseId == courseId, cancellationToken)
            ?? throw new KeyNotFoundException("Section not found or does not belong to this course.");

        var video = new Video
        {
            Title = createDto.Title,
            VideoFileId = createDto.VideoFileId,
            Provider = createDto.Provider,
            ProviderVideoId = createDto.ProviderVideoId,
            DurationSeconds = createDto.DurationSeconds,
            Transcript = createDto.Transcript,
            Status = VideoStatus.Ready,
            ViewCount = 0
        };

        await _videoRepo.AddAsync(video, cancellationToken);

        var position = await _context.SectionItems
            .CountAsync(si => si.SectionId == createDto.SectionId, cancellationToken) + 1;

        var sectionItem = new SectionItem
        {
            SectionId = createDto.SectionId,
            ItemType = SectionItemType.Video,
            ItemId = video.Id,
            Position = createDto.IsPreview ? 0 : position,
            IsPreviewAllowed = createDto.IsPreview,
            IsMandatory = true
        };

        await _sectionItemRepo.AddAsync(sectionItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Video {VideoId} created in section {SectionId} (course {CourseId})", video.Id, createDto.SectionId, courseId);

        return MapToDto(video);
    }

    public async Task<VideoResponseDto> UpdateVideoAsync(Guid id, UpdateVideoDto updateDto, CancellationToken cancellationToken = default)
    {
        var video = await _videoRepo.FirstOrDefaultAsync(
            v => v.Id == id,
            include: q => q.Include(v => v.SectionItem),
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException("Video not found.");

        video.Title = updateDto.Title;
        video.Transcript = updateDto.Transcript;

        if (video.SectionItem is not null)
        {
            video.SectionItem.IsPreviewAllowed = updateDto.IsPreview;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Video {VideoId} updated", id);

        return MapToDto(video);
    }

    public async Task<bool> DeleteVideoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var video = await _videoRepo.FirstOrDefaultAsync(
            v => v.Id == id,
            include: q => q.Include(v => v.SectionItem),
            cancellationToken: cancellationToken);

        if (video is null)
            return false;

        if (video.SectionItem is not null)
            await _sectionItemRepo.DeleteAsync(video.SectionItem, cancellationToken);

        await _videoRepo.DeleteAsync(video, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Video {VideoId} deleted", id);

        return true;
    }

    private VideoResponseDto MapToDto(Video video)
    {
        return new VideoResponseDto
        {
            Id = video.Id,
            Title = video.Title,
            VideoUrl = _objectStorage.GetPublicUrl("videos", video.VideoFileId.ToString()),
            Provider = video.Provider,
            DurationSeconds = video.DurationSeconds,
            Quality = video.Quality,
            Status = video.Status,
            IsPreview = video.SectionItem?.IsPreviewAllowed ?? false,
            ViewCount = video.ViewCount,
            Transcript = video.Transcript,
            CreatedAt = DateTime.UtcNow
        };
    }
}
