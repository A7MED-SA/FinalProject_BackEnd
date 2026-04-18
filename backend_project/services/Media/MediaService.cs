using backend_project.Configuration;
using backend_project.Data;
using backend_project.DTOs.Media;
using backend_project.Models;
using backend_project.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend_project.Services;

public class MediaService : IMediaService
{
    private readonly ApplicationDbContext _context;
    private readonly IObjectStorage _objectStorage;
    private readonly MinioSettings _settings;
    private readonly ILogger<MediaService> _logger;

    // Allowed file extensions for security validation
    private static readonly Dictionary<StoredFileType, HashSet<string>> _allowedExtensions = new()
    {
        [StoredFileType.Video] = new HashSet<string> { ".mp4", ".webm", ".mov", ".avi", ".mkv" },
        [StoredFileType.Document] = new HashSet<string> { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx" },
        [StoredFileType.Image] = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" },
        [StoredFileType.Recording] = new HashSet<string> { ".mp4", ".webm", ".mp3", ".wav" },
        [StoredFileType.Certificate] = new HashSet<string> { ".pdf", ".png", ".jpg", ".jpeg" }
    };

    public MediaService(
        ApplicationDbContext context,
        IObjectStorage objectStorage,
        IOptions<MinioSettings> settings,
        ILogger<MediaService> logger)
    {
        _context = context;
        _objectStorage = objectStorage;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<UploadUrlResponseDto> GenerateUploadUrlAsync(
        Guid userId,
        UploadUrlRequestDto request)
    {
        // Validate file type and extension
        ValidateFileExtension(request.FileName, request.FileType);

        // Determine bucket based on file type
        var bucket = GetBucketForFileType(request.FileType, request.Visibility);

        // Generate unique object key
        var objectKey = GenerateObjectKey(request.FileType, request.FileName, userId);

        // Create pending upload record
        var uploadedFile = new UploadedFile
        {
            Id = Guid.NewGuid(),
            OriginalName = request.FileName,
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

        _context.Files.Add(uploadedFile);
        await _context.SaveChangesAsync(); // Save first to get the ID

        try
        {
            // Generate presigned upload URL
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
            // Cleanup: Remove the pending record if URL generation fails
            _context.Files.Remove(uploadedFile);
            await _context.SaveChangesAsync();
            throw;
        }
    }

    public async Task<MediaFileDto> ConfirmUploadAsync(Guid userId, ConfirmUploadDto request)
    {
        var file = await _context.Files
            .FirstOrDefaultAsync(f => f.Id == request.FileId && f.UploadedBy == userId)
            ?? throw new KeyNotFoundException("File not found");

        if (file.Status != FileStatus.Uploading)
            throw new InvalidOperationException("File is not in uploading state");

        // Verify file exists in MinIO
        var exists = await _objectStorage.ObjectExistsAsync(request.Bucket, request.ObjectKey);
        if (!exists)
            throw new InvalidOperationException("File not found in storage. Upload may have failed.");

        // Get actual metadata from storage (security: verify actual content type)
        var metadata = await _objectStorage.GetObjectMetadataAsync(request.Bucket, request.ObjectKey);
        if (metadata == null)
            throw new InvalidOperationException("Failed to retrieve file metadata from storage");

        // Security: Validate actual content type matches expected type
        ValidateContentType(metadata.ContentType, file.FileType);

        // Update file with actual metadata from storage
        file.SizeBytes = metadata.SizeBytes;
        file.MimeType = metadata.ContentType;
        file.Status = FileStatus.Ready;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Upload confirmed: {FileId}, Size: {Size} bytes", file.Id, file.SizeBytes);

        return MapToDto(file);
    }

    public async Task<ViewUrlResponseDto> GetViewUrlAsync(
        Guid fileId,
        Guid? userId,
        IEnumerable<string> userRoles)
    {
        var file = await _context.Files
            .Include(f => f.Uploader)
            .FirstOrDefaultAsync(f => f.Id == fileId && f.DeletedAt == null)
            ?? throw new KeyNotFoundException("File not found");

        if (file.Status != FileStatus.Ready)
            throw new InvalidOperationException("File is not ready for viewing");

        // Check permissions
        if (!await CheckAccessPermissionAsync(file, userId, userRoles))
            throw new UnauthorizedAccessException("Access denied to this file");

        // Generate presigned view URL
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

    // ✅ FIXED: Changed from private to public to implement interface member
    public async Task<bool> CheckAccessPermissionAsync(
        UploadedFile file,
        Guid? userId,
        IEnumerable<string> userRoles)
    {
        var roles = userRoles.ToList();

        // Admin can access everything
        if (roles.Contains("Admin"))
            return true;

        switch (file.Visibility)
        {
            case FileVisibility.Public:
                return true;

            case FileVisibility.Private:
                // Only owner or admin
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
        // Optimized: Check file type first to avoid unnecessary queries
        // Check if this file is a Video
        var video = await _context.Videos
            .Include(v => v.SectionItem)
                .ThenInclude(si => si!.Section)
            .FirstOrDefaultAsync(v => v.VideoFileId == file.Id);

        if (video != null)
        {
            // Check if video is ready
            if (video.Status != VideoStatus.Ready)
                return false;

            // Check if preview is allowed
            if (video.SectionItem?.IsPreviewAllowed == true)
                return true;

            // Check enrollment
            var courseId = video.SectionItem?.Section.CourseId;
            if (courseId.HasValue)
            {
                return await IsUserEnrolledAsync(userId, courseId.Value);
            }
        }

        // Check if this file is a Document
        var document = await _context.Documents
            .Include(d => d.SectionItem)
                .ThenInclude(si => si!.Section)
            .FirstOrDefaultAsync(d => d.FileId == file.Id);

        if (document != null)
        {
            // Check if preview is allowed
            if (document.SectionItem?.IsPreviewAllowed == true)
                return true;

            // Check enrollment
            var courseId = document.SectionItem?.Section.CourseId;
            if (courseId.HasValue)
            {
                return await IsUserEnrolledAsync(userId, courseId.Value);
            }
        }

        // Check if this is a LiveSession recording
        var liveSession = await _context.LiveSessions
            .Include(ls => ls.SectionItem)
                .ThenInclude(si => si!.Section)
            .FirstOrDefaultAsync(ls => ls.RecordingFileId == file.Id);

        if (liveSession != null)
        {
            // Must be finished to access recording
            if (liveSession.Status != LiveSessionStatus.Finished)
                return false;

            var courseId = liveSession.SectionItem?.Section.CourseId ?? liveSession.CourseId;
            return await IsUserEnrolledAsync(userId, courseId);
        }

        // Check if this is a Course image or intro video (always accessible if course is public)
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => 
                c.CourseImageFileId == file.Id || 
                c.IntroVideoFileId == file.Id);

        if (course != null && course.IsPublished)
            return true;

        // Default: deny access
        return false;
    }

    private async Task<bool> IsUserEnrolledAsync(Guid userId, Guid courseId)
    {
        return await _context.Enrollments
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
            return; // Skip validation if content type not available

        // Security: Verify that the actual content type matches the expected file type
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