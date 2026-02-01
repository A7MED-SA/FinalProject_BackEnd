using backend_project.Data;
using backend_project.DTOs.Media;
using backend_project.Models;
using backend_project.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_project.Services;

public class AdminMediaService : IAdminMediaService
{
    private readonly ApplicationDbContext _context;
    private readonly IObjectStorage _objectStorage;
    private readonly ILogger<AdminMediaService> _logger;

    public AdminMediaService(
        ApplicationDbContext context,
        IObjectStorage objectStorage,
        ILogger<AdminMediaService> logger)
    {
        _context = context;
        _objectStorage = objectStorage;
        _logger = logger;
    }

    public async Task<PagedResult<MediaFileDto>> ListMediaAsync(MediaFilterDto filter)
    {
        var query = _context.Files
            .Include(f => f.Uploader)
            .AsQueryable();

        // Apply filters
        if (!filter.IncludeDeleted)
            query = query.Where(f => f.DeletedAt == null);

        if (filter.FileType.HasValue)
            query = query.Where(f => f.FileType == filter.FileType.Value);

        if (!string.IsNullOrEmpty(filter.Bucket))
            query = query.Where(f => f.Bucket == filter.Bucket);

        if (filter.Visibility.HasValue)
            query = query.Where(f => f.Visibility == filter.Visibility.Value);

        if (filter.UploadedBy.HasValue)
            query = query.Where(f => f.UploadedBy == filter.UploadedBy.Value);

        if (filter.Status.HasValue)
            query = query.Where(f => f.Status == filter.Status.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(f => f.UploadedAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(f => f.UploadedAt <= filter.ToDate.Value);

        if (!string.IsNullOrEmpty(filter.SearchTerm))
            query = query.Where(f => f.OriginalName.Contains(filter.SearchTerm));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(f => f.UploadedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(f => new MediaFileDto
            {
                Id = f.Id,
                OriginalName = f.OriginalName,
                FilePath = f.FilePath,
                Bucket = f.Bucket,
                FileType = f.FileType,
                Visibility = f.Visibility,
                Status = f.Status,
                MimeType = f.MimeType,
                SizeBytes = f.SizeBytes,
                UploadedAt = f.UploadedAt,
                UploadedBy = f.UploadedBy,
                UploaderName = f.Uploader != null ? f.Uploader.Name : null
            })
            .ToListAsync();

        return new PagedResult<MediaFileDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<MediaDetailDto> GetMediaDetailsAsync(Guid fileId)
    {
        var file = await _context.Files
            .Include(f => f.Uploader)
            .FirstOrDefaultAsync(f => f.Id == fileId)
            ?? throw new KeyNotFoundException("File not found");

        var dto = new MediaDetailDto
        {
            Id = file.Id,
            OriginalName = file.OriginalName,
            FilePath = file.FilePath,
            Bucket = file.Bucket,
            FileType = file.FileType,
            Visibility = file.Visibility,
            Status = file.Status,
            MimeType = file.MimeType,
            SizeBytes = file.SizeBytes,
            UploadedAt = file.UploadedAt,
            UploadedBy = file.UploadedBy,
            UploaderName = file.Uploader?.Name,
            DeletedAt = file.DeletedAt,
            StorageProvider = file.StorageProvider
        };

        // Load related entities
        await LoadRelatedEntitiesAsync(dto, file);

        return dto;
    }

    private async Task LoadRelatedEntitiesAsync(MediaDetailDto dto, UploadedFile file)
    {
        // Check for Video
        var video = await _context.Videos
            .Include(v => v.SectionItem)
                .ThenInclude(si => si!.Section)
                    .ThenInclude(s => s.Course)
            .FirstOrDefaultAsync(v => v.VideoFileId == file.Id);

        if (video != null)
        {
            dto.Video = new RelatedVideoDto
            {
                Id = video.Id,
                Title = video.Title,
                Status = video.Status,
                DurationSeconds = video.DurationSeconds,
                SectionId = video.SectionItem?.SectionId,
                SectionTitle = video.SectionItem?.Section.Title,
                CourseId = video.SectionItem?.Section.CourseId,
                CourseTitle = video.SectionItem?.Section.Course.Title
            };
            return;
        }

        // Check for Document
        var document = await _context.Documents
            .Include(d => d.SectionItem)
                .ThenInclude(si => si!.Section)
                    .ThenInclude(s => s.Course)
            .FirstOrDefaultAsync(d => d.FileId == file.Id);

        if (document != null)
        {
            dto.Document = new RelatedDocumentDto
            {
                Id = document.Id,
                Title = document.Title,
                SectionId = document.SectionItem?.SectionId,
                SectionTitle = document.SectionItem?.Section.Title,
                CourseId = document.SectionItem?.Section.CourseId,
                CourseTitle = document.SectionItem?.Section.Course.Title
            };
            return;
        }

        // Check for Course image/video
        var course = await _context.Courses
            .FirstOrDefaultAsync(c =>
                c.CourseImageFileId == file.Id ||
                c.IntroVideoFileId == file.Id);

        if (course != null)
        {
            dto.Course = new RelatedCourseDto
            {
                Id = course.Id,
                Title = course.Title,
                UsageType = course.CourseImageFileId == file.Id ? "Image" : "IntroVideo"
            };
        }
    }

    public async Task SoftDeleteAsync(Guid fileId, Guid adminId)
    {
        var file = await _context.Files
            .FirstOrDefaultAsync(f => f.Id == fileId && f.DeletedAt == null)
            ?? throw new KeyNotFoundException("File not found");

        file.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("File {FileId} soft deleted by admin {AdminId}", fileId, adminId);
    }

    public async Task RestoreAsync(Guid fileId, Guid adminId)
    {
        var file = await _context.Files
            .FirstOrDefaultAsync(f => f.Id == fileId && f.DeletedAt != null)
            ?? throw new KeyNotFoundException("File not found or not deleted");

        file.DeletedAt = null;
        await _context.SaveChangesAsync();

        _logger.LogInformation("File {FileId} restored by admin {AdminId}", fileId, adminId);
    }

    public async Task PermanentDeleteAsync(Guid fileId, Guid adminId)
    {
        var file = await _context.Files
            .FirstOrDefaultAsync(f => f.Id == fileId)
            ?? throw new KeyNotFoundException("File not found");

        // Delete from MinIO
        try
        {
            await _objectStorage.DeleteObjectAsync(file.Bucket, file.FilePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete object from storage: {Bucket}/{Key}", 
                file.Bucket, file.FilePath);
        }

        // Delete from DB
        _context.Files.Remove(file);
        await _context.SaveChangesAsync();

        _logger.LogWarning("File {FileId} permanently deleted by admin {AdminId}", fileId, adminId);
    }

    public async Task<StorageStatsDto> GetStorageStatsAsync()
    {
        var files = await _context.Files
            .Where(f => f.DeletedAt == null)
            .GroupBy(f => new { f.Bucket, f.FileType })
            .Select(g => new
            {
                g.Key.Bucket,
                g.Key.FileType,
                Count = g.Count(),
                TotalSize = g.Sum(f => f.SizeBytes)
            })
            .ToListAsync();

        var bucketStats = files
            .GroupBy(f => f.Bucket)
            .ToDictionary(
                g => g.Key,
                g => new BucketStats
                {
                    FilesCount = g.Sum(x => x.Count),
                    SizeBytes = g.Sum(x => x.TotalSize)
                });

        var fileTypeCounts = files
            .GroupBy(f => f.FileType)
            .ToDictionary(
                g => g.Key,
                g => (long)g.Sum(x => x.Count));

        return new StorageStatsDto
        {
            TotalFilesCount = files.Sum(f => f.Count),
            TotalSizeBytes = files.Sum(f => f.TotalSize),
            BucketStats = bucketStats,
            FileTypeCounts = fileTypeCounts
        };
    }
}
