using System.Collections.Generic;
using Athary.Application.DTOs.Media;
using Athary.Application.Interfaces.Media;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Microsoft.Extensions.Logging;
using Athary.Domain.Interfaces;
using Athary.Infrastructure.Data;
using Athary.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Athary.Infrastructure.Services.Media;

public sealed class MediaService : IMediaService
{
    private readonly IRepository<UploadedFile> _fileRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IObjectStorage _objectStorage;
    private readonly MinioSettings _settings;
    private readonly ILogger<MediaService> _logger;
    private readonly ApplicationDbContext _dbContext;

    private static readonly Dictionary<StoredFileType, HashSet<string>> _allowedExtensions = new()
    {
        [StoredFileType.Video] = new HashSet<string> { ".mp4", ".webm", ".mov", ".avi", ".mkv" },
        [StoredFileType.Document] = new HashSet<string> { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx" },
        [StoredFileType.Image] = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" },
        [StoredFileType.Recording] = new HashSet<string> { ".mp4", ".webm", ".mp3", ".wav" },
        [StoredFileType.Certificate] = new HashSet<string> { ".pdf", ".png", ".jpg", ".jpeg" }
    };

    public MediaService(
        IRepository<UploadedFile> fileRepo,
        IUnitOfWork unitOfWork,
        IObjectStorage objectStorage,
        IOptions<MinioSettings> settings,
        ILogger<MediaService> logger,
        ApplicationDbContext dbContext)
    {
        _fileRepo = fileRepo;
        _unitOfWork = unitOfWork;
        _objectStorage = objectStorage;
        _settings = settings.Value;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<UploadUrlResponseDto> GenerateUploadUrlAsync(
        Guid userId,
        UploadUrlRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateFileExtension(request.FileName, request.FileType);

        var bucket = GetBucketForFileType(request.FileType, request.Visibility);
        var objectKey = GenerateObjectKey(request.FileType, request.FileName, userId);

        var uploadedFile = new UploadedFile
        {
            OriginalName = request.FileName,
            FileName = SanitizeFileName(request.FileName),
            FilePath = objectKey,
            Bucket = bucket,
            FileType = request.FileType,
            Visibility = request.Visibility,
            Status = FileStatus.Uploading,
            MimeType = request.ContentType,
            SizeBytes = request.FileSizeBytes,
            UploadedBy = userId,
            UploadedAt = DateTime.UtcNow
        };

        await _fileRepo.AddAsync(uploadedFile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var presignedResult = await _objectStorage.GenerateUploadUrlAsync(
                bucket,
                objectKey,
                request.ContentType,
                request.FileSizeBytes);

            return new UploadUrlResponseDto
            {
                FileId = uploadedFile.Id,
                UploadUrl = presignedResult.UploadUrl,
                ObjectKey = objectKey,
                Bucket = bucket,
                ExpiresAt = presignedResult.ExpiresAt,
                RequiredHeaders = presignedResult.RequiredHeaders
            };
        }
        catch (Exception)
        {
            await _fileRepo.DeleteAsync(uploadedFile, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    public async Task<MediaFileDto> ConfirmUploadAsync(
        Guid userId,
        ConfirmUploadDto request,
        CancellationToken cancellationToken = default)
    {
        var file = await _fileRepo.GetByIdAsync(request.FileId, cancellationToken)
            ?? throw new KeyNotFoundException("File not found");

        if (file.UploadedBy != userId)
            throw new KeyNotFoundException("File not found");

        if (file.Status != FileStatus.Uploading)
            throw new InvalidOperationException("File is not in uploading state");

        var exists = await _objectStorage.ObjectExistsAsync(request.Bucket, request.ObjectKey);
        if (!exists)
            throw new InvalidOperationException("File not found in storage. Upload may have failed.");

        var metadata = await _objectStorage.GetObjectMetadataAsync(request.Bucket, request.ObjectKey);
        if (metadata == null)
            throw new InvalidOperationException("Failed to retrieve file metadata from storage");

        ValidateContentType(metadata.ContentType, file.FileType);

        file.SizeBytes = metadata.SizeBytes;
        file.MimeType = metadata.ContentType;
        file.Status = FileStatus.Ready;

        await _fileRepo.UpdateAsync(file, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Upload confirmed: {FileId}, Size: {Size} bytes", file.Id, file.SizeBytes);

        return MapToDto(file);
    }

    public async Task<ViewUrlResponseDto> GetViewUrlAsync(
        Guid fileId,
        Guid? userId,
        IEnumerable<string> userRoles,
        CancellationToken cancellationToken = default)
    {
        var file = await _dbContext.Files
            .Include(f => f.Uploader)
            .FirstOrDefaultAsync(f => f.Id == fileId, cancellationToken)
            ?? throw new KeyNotFoundException("File not found");

        if (file.Status != FileStatus.Ready)
            throw new InvalidOperationException("File is not ready for viewing");

        if (!await CheckAccessPermissionAsync(file, userId, userRoles, cancellationToken))
            throw new UnauthorizedAccessException("Access denied to this file");

        var viewUrl = await _objectStorage.GenerateViewUrlAsync(
            file.Bucket,
            file.FilePath,
            _settings.PresignedUrlExpiryMinutes);

        return new ViewUrlResponseDto
        {
            FileId = file.Id,
            ViewUrl = viewUrl,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_settings.PresignedUrlExpiryMinutes),
            ContentType = file.MimeType ?? "application/octet-stream",
            SizeBytes = file.SizeBytes
        };
    }

    public async Task<bool> CheckAccessPermissionAsync(
        UploadedFile file,
        Guid? userId,
        IEnumerable<string> userRoles,
        CancellationToken cancellationToken = default)
    {
        var roles = userRoles.ToList();

        if (roles.Contains("Admin"))
            return true;

        switch (file.Visibility)
        {
            case FileVisibility.Public:
                return true;

            case FileVisibility.Private:
                return userId.HasValue && file.UploadedBy == userId.Value;

            case FileVisibility.EnrolledOnly:
                if (!userId.HasValue)
                    return false;
                return await CheckEnrollmentAccessAsync(file, userId.Value);

            default:
                return false;
        }
    }

    private async Task<bool> CheckEnrollmentAccessAsync(UploadedFile file, Guid userId)
    {
        var video = await _dbContext.Videos
            .Include(v => v.SectionItem)
                .ThenInclude(si => si!.Section)
            .FirstOrDefaultAsync(v => v.VideoFileId == file.Id);

        if (video != null)
        {
            if (video.Status != VideoStatus.Ready)
                return false;

            if (video.SectionItem?.IsPreviewAllowed == true)
                return true;

            var courseId = video.SectionItem?.Section.CourseId;
            if (courseId.HasValue)
                return await IsUserEnrolledAsync(userId, courseId.Value);
        }

        var document = await _dbContext.Documents
            .Include(d => d.SectionItem)
                .ThenInclude(si => si!.Section)
            .FirstOrDefaultAsync(d => d.FileId == file.Id);

        if (document != null)
        {
            if (document.SectionItem?.IsPreviewAllowed == true)
                return true;

            var courseId = document.SectionItem?.Section.CourseId;
            if (courseId.HasValue)
                return await IsUserEnrolledAsync(userId, courseId.Value);
        }

        var liveSession = await _dbContext.LiveSessions
            .Include(ls => ls.SectionItem)
                .ThenInclude(si => si!.Section)
            .FirstOrDefaultAsync(ls => ls.RecordingFileId == file.Id);

        if (liveSession != null)
        {
            if (liveSession.Status != LiveSessionStatus.Finished)
                return false;

            var courseId = liveSession.SectionItem?.Section.CourseId ?? liveSession.CourseId;
            return await IsUserEnrolledAsync(userId, courseId);
        }

        var course = await _dbContext.Courses
            .FirstOrDefaultAsync(c =>
                c.CourseImageFileId == file.Id ||
                c.IntroVideoFileId == file.Id);

        if (course != null && course.IsPublished)
            return true;

        return false;
    }

    private async Task<bool> IsUserEnrolledAsync(Guid userId, Guid courseId)
    {
        return await _dbContext.Enrollments
            .AnyAsync(e =>
                e.UserId == userId &&
                e.CourseId == courseId &&
                e.Status == EnrollmentStatus.InProgress &&
                (e.AccessExpiresAt == null || e.AccessExpiresAt > DateTime.UtcNow) &&
                !e.IsRefunded);
    }

    private string GetBucketForFileType(StoredFileType fileType, FileVisibility visibility)
    {
        if (visibility == FileVisibility.Private)
            return _settings.PrivateBucket;

        return fileType switch
        {
            StoredFileType.Video => _settings.VideosBucket,
            StoredFileType.Document => _settings.DocumentsBucket,
            StoredFileType.Image => _settings.ImagesBucket,
            StoredFileType.Recording => _settings.RecordingsBucket,
            StoredFileType.Certificate => _settings.CertificatesBucket,
            _ => _settings.PrivateBucket
        };
    }

    private static string GenerateObjectKey(StoredFileType fileType, string fileName, Guid userId)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var extension = Path.GetExtension(fileName);
        var sanitizedName = SanitizeFileName(Path.GetFileNameWithoutExtension(fileName));

        return $"{fileType.ToString().ToLower()}/{timestamp}/{userId:N}/{sanitizedName}_{uniqueId}{extension}";
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries))
            .Replace(" ", "_")
            .ToLowerInvariant();
    }

    private void ValidateFileExtension(string fileName, StoredFileType fileType)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!_allowedExtensions.TryGetValue(fileType, out var allowedExtensions))
            throw new ArgumentException($"Unsupported file type: {fileType}");

        if (!allowedExtensions.Contains(extension))
            throw new ArgumentException($"File extension '{extension}' is not allowed for file type '{fileType}'. Allowed extensions: {string.Join(", ", allowedExtensions)}");
    }

    private void ValidateContentType(string contentType, StoredFileType fileType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return;

        var contentTypeLower = contentType.ToLowerInvariant();

        bool isValid = fileType switch
        {
            StoredFileType.Video => contentTypeLower.StartsWith("video/"),
            StoredFileType.Document => contentTypeLower.StartsWith("application/pdf") ||
                                    contentTypeLower.StartsWith("application/msword") ||
                                    contentTypeLower.StartsWith("application/vnd.openxmlformats-officedocument"),
            StoredFileType.Image => contentTypeLower.StartsWith("image/"),
            StoredFileType.Recording => contentTypeLower.StartsWith("video/") || contentTypeLower.StartsWith("audio/"),
            StoredFileType.Certificate => contentTypeLower.StartsWith("application/pdf") || contentTypeLower.StartsWith("image/"),
            _ => true
        };

        if (!isValid)
            throw new InvalidOperationException($"Content type '{contentType}' does not match expected type for '{fileType}'. Upload rejected for security reasons.");
    }

    private static MediaFileDto MapToDto(UploadedFile file)
    {
        return new MediaFileDto
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
            UploaderName = file.Uploader?.FullName
        };
    }
}
