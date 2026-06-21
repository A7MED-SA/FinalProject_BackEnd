using Athary.Application.DTOs.Media;
using Athary.Application.Interfaces.Media;
using Microsoft.Extensions.Logging;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Domain.Interfaces;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.Media;

public sealed class AdminMediaService : IAdminMediaService
{
    private readonly IRepository<UploadedFile> _fileRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IObjectStorage _objectStorage;
    private readonly ILogger<AdminMediaService> _logger;
    private readonly ApplicationDbContext _dbContext;

    public AdminMediaService(
        IRepository<UploadedFile> fileRepo,
        IUnitOfWork unitOfWork,
        IObjectStorage objectStorage,
        ILogger<AdminMediaService> logger,
        ApplicationDbContext dbContext)
    {
        _fileRepo = fileRepo;
        _unitOfWork = unitOfWork;
        _objectStorage = objectStorage;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<PagedResult<MediaFileDto>> ListMediaAsync(MediaFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Files
            .Include(f => f.Uploader)
            .AsQueryable();

        if (filter.IncludeDeleted)
            query = query.IgnoreQueryFilters();

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

        var totalCount = await query.CountAsync(cancellationToken);

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
                UploaderName = f.Uploader != null ? f.Uploader.FullName : null
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<MediaFileDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<MediaDetailDto> GetMediaDetailsAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var file = await _dbContext.Files
            .Include(f => f.Uploader)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.Id == fileId, cancellationToken)
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
            UploaderName = file.Uploader?.FullName,
            DeletedAt = file.DeletedAt,
            StorageProvider = file.StorageProvider
        };

        var (relatedVideo, relatedDocument, relatedCourse) = await LoadRelatedEntitiesAsync(file);

        return dto with { Video = relatedVideo, Document = relatedDocument, Course = relatedCourse };
    }

    public async Task SoftDeleteAsync(Guid fileId, Guid adminId, CancellationToken cancellationToken = default)
    {
        var file = await _fileRepo.GetByIdAsync(fileId, cancellationToken)
            ?? throw new KeyNotFoundException("File not found");

        if (file.DeletedAt != null)
            throw new KeyNotFoundException("File not found");

        file.DeletedAt = DateTime.UtcNow;

        await _fileRepo.UpdateAsync(file, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("File {FileId} soft deleted by admin {AdminId}", fileId, adminId);
    }

    public async Task RestoreAsync(Guid fileId, Guid adminId, CancellationToken cancellationToken = default)
    {
        var file = await _fileRepo.GetByIdAsync(fileId, cancellationToken)
            ?? throw new KeyNotFoundException("File not found");

        if (file.DeletedAt == null)
            throw new KeyNotFoundException("File not found or not deleted");

        file.DeletedAt = null;

        await _fileRepo.UpdateAsync(file, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("File {FileId} restored by admin {AdminId}", fileId, adminId);
    }

    public async Task PermanentDeleteAsync(Guid fileId, Guid adminId, CancellationToken cancellationToken = default)
    {
        var file = await _fileRepo.GetByIdAsync(fileId, cancellationToken)
            ?? throw new KeyNotFoundException("File not found");

        try
        {
            await _objectStorage.DeleteObjectAsync(file.Bucket, file.FilePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete object from storage: {Bucket}/{Key}",
                file.Bucket, file.FilePath);
        }

        await _fileRepo.DeleteAsync(file, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("File {FileId} permanently deleted by admin {AdminId}", fileId, adminId);
    }

    public async Task<StorageStatsDto> GetStorageStatsAsync(CancellationToken cancellationToken = default)
    {
        var files = await _dbContext.Files
            .GroupBy(f => new { f.Bucket, f.FileType })
            .Select(g => new
            {
                g.Key.Bucket,
                g.Key.FileType,
                Count = g.Count(),
                TotalSize = g.Sum(f => f.SizeBytes)
            })
            .ToListAsync(cancellationToken);

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

    private async Task<(RelatedVideoDto? Video, RelatedDocumentDto? Document, RelatedCourseDto? Course)> LoadRelatedEntitiesAsync(UploadedFile file)
    {
        var video = await _dbContext.Videos
            .Include(v => v.SectionItem)
                .ThenInclude(si => si!.Section)
                    .ThenInclude(s => s.Course)
            .FirstOrDefaultAsync(v => v.VideoFileId == file.Id);

        if (video != null)
        {
            return (
                new RelatedVideoDto
                {
                    Id = video.Id,
                    Title = video.Title,
                    Status = video.Status,
                    DurationSeconds = video.DurationSeconds,
                    SectionId = video.SectionItem?.SectionId,
                    SectionTitle = video.SectionItem?.Section.Title,
                    CourseId = video.SectionItem?.Section.CourseId,
                    CourseTitle = video.SectionItem?.Section.Course.Title
                },
                null, null
            );
        }

        var document = await _dbContext.Documents
            .Include(d => d.SectionItem)
                .ThenInclude(si => si!.Section)
                    .ThenInclude(s => s.Course)
            .FirstOrDefaultAsync(d => d.FileId == file.Id);

        if (document != null)
        {
            return (
                null,
                new RelatedDocumentDto
                {
                    Id = document.Id,
                    Title = document.Title,
                    SectionId = document.SectionItem?.SectionId,
                    SectionTitle = document.SectionItem?.Section.Title,
                    CourseId = document.SectionItem?.Section.CourseId,
                    CourseTitle = document.SectionItem?.Section.Course.Title
                },
                null
            );
        }

        var course = await _dbContext.Courses
            .FirstOrDefaultAsync(c =>
                c.CourseImageFileId == file.Id ||
                c.IntroVideoFileId == file.Id);

        if (course != null)
        {
            return (
                null, null,
                new RelatedCourseDto
                {
                    Id = course.Id,
                    Title = course.Title,
                    UsageType = course.CourseImageFileId == file.Id ? "Image" : "IntroVideo"
                }
            );
        }

        return (null, null, null);
    }
}
