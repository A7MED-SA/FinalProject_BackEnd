using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("files")]
public class UploadedFile : BaseEntity
{

    [Required]
    [Column("uploaded_by")]
    public Guid UploadedBy { get; set; }

    [Required]
    [Column("file_path")]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [Column("file_name")]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [Column("original_name")]
    [MaxLength(255)]
    public string OriginalName { get; set; } = string.Empty;

    [Column("file_type")]
    public StoredFileType FileType { get; set; }

    [Required]
    [Column("bucket")]
    [MaxLength(100)]
    public string Bucket { get; set; } = string.Empty;

    [Column("status")]
    public FileStatus Status { get; set; } = FileStatus.Uploading;
    
    [Column("storage_provider")]
    [MaxLength(50)]
    public string StorageProvider { get; set; } = "local";

    [Column("mime_type")]
    [MaxLength(100)]
    public string? MimeType { get; set; }

    [Column("size_bytes")]
    public long SizeBytes { get; set; }

    [Column("visibility")]
    public FileVisibility Visibility { get; set; }

    [Column("uploaded_at")]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    // Navigation Properties
    [ForeignKey("UploadedBy")]
    public virtual User Uploader { get; set; } = null!;
}

public enum StoredFileType
{
    Image,
    Video,
    Document,
    Recording,
    Certificate
}

public enum FileVisibility
{
    Public,
    EnrolledOnly,
    Private
}
public enum FileStatus
{
    Uploading,
    Ready,
    Failed,
    Deleted
}


