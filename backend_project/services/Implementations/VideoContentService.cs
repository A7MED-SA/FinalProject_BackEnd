using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.Video;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class VideoContentService : IVideoContentService
{
    private readonly ApplicationDbContext _context;

    public VideoContentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VideoResponseDto> GetVideoAsync(Guid id)
    {
        var video = await _context.Videos
            .AsNoTracking()
            .Include(v => v.SectionItem)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (video == null)
            throw new KeyNotFoundException("Video not found.");

        return MapToDto(video);
    }

    public async Task<VideoResponseDto> CreateVideoAsync(CreateVideoDto createDto)
    {
        var section = await _context.Sections
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == createDto.SectionId);

        if (section == null)
            throw new KeyNotFoundException("Section not found.");

        var video = new Video
        {
            Id = Guid.NewGuid(),
            Title = createDto.Title,
            VideoFileId = createDto.VideoFileId,
            Provider = createDto.Provider,
            DurationSeconds = createDto.DurationSeconds,
            Transcript = createDto.Transcript,
            Status = VideoStatus.Ready,
            ViewCount = 0
        };

        _context.Videos.Add(video);

        var sectionItem = new SectionItem
        {
            Id = Guid.NewGuid(),
            SectionId = createDto.SectionId,
            ItemType = SectionItemType.Video,
            ItemId = video.Id,
            Position = createDto.IsPreview ? 0 : await _context.SectionItems
                .Where(si => si.SectionId == createDto.SectionId)
                .CountAsync() + 1,
            IsPreviewAllowed = createDto.IsPreview,
            IsMandatory = true
        };

        _context.SectionItems.Add(sectionItem);
        await _context.SaveChangesAsync();

        return await GetVideoAsync(video.Id);
    }

    public async Task<VideoResponseDto> UpdateVideoAsync(Guid id, UpdateVideoDto updateDto)
    {
        var video = await _context.Videos
            .Include(v => v.SectionItem)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (video == null)
            throw new KeyNotFoundException("Video not found.");

        video.Title = updateDto.Title;
        video.Transcript = updateDto.Transcript;

        if (video.SectionItem != null)
        {
            video.SectionItem.IsPreviewAllowed = updateDto.IsPreview;
        }

        await _context.SaveChangesAsync();

        return MapToDto(video);
    }

    public async Task<bool> DeleteVideoAsync(Guid id)
    {
        var video = await _context.Videos
            .Include(v => v.SectionItem)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (video == null)
            return false;

        if (video.SectionItem != null)
            _context.SectionItems.Remove(video.SectionItem);

        _context.Videos.Remove(video);
        await _context.SaveChangesAsync();

        return true;
    }

    private static VideoResponseDto MapToDto(Video video)
    {
        return new VideoResponseDto
        {
            Id = video.Id,
            Title = video.Title,
            VideoUrl = string.Empty,
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
