using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("documents")]
public class Document : BaseEntity
{
    [Required, MaxLength(255)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Required]
    [Column("file_id")]
    public Guid FileId { get; set; }

    [Column("download_count")]
    public int DownloadCount { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(FileId))]
    public UploadedFile File { get; set; } = null!;

    // Navigation to SectionItem (if document is part of a course section)
    public virtual SectionItem? SectionItem { get; set; }
}


public enum DocumentFileType
{
    Pdf,
    Zip,
    Doc,
    Ppt,
    Excel
}
