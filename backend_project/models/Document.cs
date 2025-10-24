using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("documents")]
public class Document : BaseEntity
{

    [Required]
    [Column("title")]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Required]
    [Column("file_url")]
    [MaxLength(500)]
    public string FileUrl { get; set; } = string.Empty;

    [Column("file_type")]
    [MaxLength(10)]
    public DocumentFileType FileType { get; set; }

    [Column("file_size_kb")]
    public int FileSizeKb { get; set; } = 0;

    [Column("download_count")]
    public int DownloadCount { get; set; } = 0;

    [Column("uploaded_at")]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum DocumentFileType
{
    Pdf,
    Zip,
    Doc,
    Ppt,
    Excel
}
