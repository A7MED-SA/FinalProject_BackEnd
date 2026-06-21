using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class UploadedFile : BaseEntity
{
    public Guid UploadedBy { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public StoredFileType FileType { get; set; }
    public string Bucket { get; set; } = string.Empty;
    public FileStatus Status { get; set; } = FileStatus.Uploading;
    public string StorageProvider { get; set; } = "local";
    public string? MimeType { get; set; }
    public long SizeBytes { get; set; }
    public FileVisibility Visibility { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public User Uploader { get; set; } = null!;
}
