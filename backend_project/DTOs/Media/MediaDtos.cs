using backend_project.Models;

namespace backend_project.DTOs.Media;

// ========== Request DTOs ==========

public class UploadUrlRequestDto
{
    public StoredFileType FileType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public FileVisibility Visibility { get; set; } = FileVisibility.Private;

    /// <summary>
    /// Optional: Related entity (Course, Video, Document, etc.)
    /// </summary>
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
}

public class ConfirmUploadDto
{
    public Guid FileId { get; set; }
    public string ObjectKey { get; set; } = string.Empty;
    public string Bucket { get; set; } = string.Empty;
}

public class MediaFilterDto
{
    public StoredFileType? FileType { get; set; }
    public string? Bucket { get; set; }
    public FileVisibility? Visibility { get; set; }
    public Guid? UploadedBy { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public FileStatus? Status { get; set; }
    public bool IncludeDeleted { get; set; } = false;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
}

// ========== Response DTOs ==========

public class UploadUrlResponseDto
{
    public Guid FileId { get; set; }
    public string UploadUrl { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public string Bucket { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public Dictionary<string, string> RequiredHeaders { get; set; } = new();
}

public class ViewUrlResponseDto
{
    public Guid FileId { get; set; }
    public string ViewUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
}

public class MediaFileDto
{
    public Guid Id { get; set; }
    public string OriginalName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Bucket { get; set; } = string.Empty;
    public StoredFileType FileType { get; set; }
    public FileVisibility Visibility { get; set; }
    public FileStatus Status { get; set; }
    public string? MimeType { get; set; }
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    public Guid UploadedBy { get; set; }
    public string? UploaderName { get; set; }
}

public class MediaDetailDto : MediaFileDto
{
    public DateTime? DeletedAt { get; set; }
    public string? StorageProvider { get; set; }

    // Related entities
    public RelatedVideoDto? Video { get; set; }
    public RelatedDocumentDto? Document { get; set; }
    public RelatedCourseDto? Course { get; set; }
}

public class RelatedVideoDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public VideoStatus Status { get; set; }
    public int DurationSeconds { get; set; }
    public Guid? SectionId { get; set; }
    public string? SectionTitle { get; set; }
    public Guid? CourseId { get; set; }
    public string? CourseTitle { get; set; }
}

public class RelatedDocumentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? SectionId { get; set; }
    public string? SectionTitle { get; set; }
    public Guid? CourseId { get; set; }
    public string? CourseTitle { get; set; }
}

public class RelatedCourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? UsageType { get; set; } // "Image" or "IntroVideo"
}

public class StorageStatsDto
{
    public long TotalFilesCount { get; set; }
    public long TotalSizeBytes { get; set; }
    public Dictionary<string, BucketStats> BucketStats { get; set; } = new();
    public Dictionary<StoredFileType, long> FileTypeCounts { get; set; } = new();
}

public class BucketStats
{
    public long FilesCount { get; set; }
    public long SizeBytes { get; set; }
}
