using Athary.Domain.Enums;

namespace Athary.Application.DTOs.Media;

public sealed record UploadUrlRequestDto
{
    public StoredFileType FileType { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public FileVisibility Visibility { get; init; } = FileVisibility.Private;
    public Guid? RelatedEntityId { get; init; }
    public string? RelatedEntityType { get; init; }
}

public sealed record ConfirmUploadDto
{
    public Guid FileId { get; init; }
    public string ObjectKey { get; init; } = string.Empty;
    public string Bucket { get; init; } = string.Empty;
}

public sealed record MediaFilterDto
{
    public StoredFileType? FileType { get; init; }
    public string? Bucket { get; init; }
    public FileVisibility? Visibility { get; init; }
    public Guid? UploadedBy { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public FileStatus? Status { get; init; }
    public bool IncludeDeleted { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
}

public sealed record UploadUrlResponseDto
{
    public Guid FileId { get; init; }
    public string UploadUrl { get; init; } = string.Empty;
    public string ObjectKey { get; init; } = string.Empty;
    public string Bucket { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public Dictionary<string, string> RequiredHeaders { get; init; } = new();
}

public sealed record ViewUrlResponseDto
{
    public Guid FileId { get; init; }
    public string ViewUrl { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public string ContentType { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
}

public record MediaFileDto
{
    public Guid Id { get; init; }
    public string OriginalName { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public string Bucket { get; init; } = string.Empty;
    public StoredFileType FileType { get; init; }
    public FileVisibility Visibility { get; init; }
    public FileStatus Status { get; init; }
    public string? MimeType { get; init; }
    public long SizeBytes { get; init; }
    public DateTime UploadedAt { get; init; }
    public Guid UploadedBy { get; init; }
    public string? UploaderName { get; init; }
}

public sealed record MediaDetailDto : MediaFileDto
{
    public DateTime? DeletedAt { get; init; }
    public string? StorageProvider { get; init; }
    public RelatedVideoDto? Video { get; init; }
    public RelatedDocumentDto? Document { get; init; }
    public RelatedCourseDto? Course { get; init; }
}

public sealed record RelatedVideoDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public VideoStatus Status { get; init; }
    public int DurationSeconds { get; init; }
    public Guid? SectionId { get; init; }
    public string? SectionTitle { get; init; }
    public Guid? CourseId { get; init; }
    public string? CourseTitle { get; init; }
}

public sealed record RelatedDocumentDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public Guid? SectionId { get; init; }
    public string? SectionTitle { get; init; }
    public Guid? CourseId { get; init; }
    public string? CourseTitle { get; init; }
}

public sealed record RelatedCourseDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? UsageType { get; init; }
}

public sealed record StorageStatsDto
{
    public long TotalFilesCount { get; init; }
    public long TotalSizeBytes { get; init; }
    public Dictionary<string, BucketStats> BucketStats { get; init; } = new();
    public Dictionary<StoredFileType, long> FileTypeCounts { get; init; } = new();
}

public sealed record BucketStats
{
    public long FilesCount { get; init; }
    public long SizeBytes { get; init; }
}
