using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("content_progresses")]
public class ContentProgress : BaseEntity
{

    [Required]
    [Column("enrollment_id")]
    public Guid EnrollmentId { get; set; }

    [Column("content_type")]
    [MaxLength(20)]
    public ContentType ContentType { get; set; }

    [Required]
    [Column("content_id")]
    public Guid ContentId { get; set; }

    [Column("is_completed")]
    public bool IsCompleted { get; set; } = false;

    [Column("watch_time_seconds")]
    public int WatchTimeSeconds { get; set; } = 0;

    [Column("attempts_count")]
    public int AttemptsCount { get; set; } = 0;

    [Column("completion_percentage", TypeName = "decimal(5,2)")]
    public decimal CompletionPercentage { get; set; } = 0;

    [Column("metadata", TypeName = "nvarchar(max)")]
    public string? Metadata { get; set; }

    [Column("last_accessed_at")]
    public DateTime? LastAccessedAt { get; set; }

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    // Navigation Properties
    [ForeignKey("EnrollmentId")]
    public virtual Enrollment Enrollment { get; set; } = null!;
}

public enum ContentType
{
    Video,
    Quiz,
    Document,
    LiveSession
}
