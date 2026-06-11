using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("reports")]
public class Report : BaseEntity
{

    [Required]
    [Column("reporter_id")]
    public Guid ReporterId { get; set; }

    [Column("entity_type")]
    [MaxLength(20)]
    public ReportEntityType EntityType { get; set; }

    [Required]
    [Column("entity_id")]
    public Guid EntityId { get; set; }

    [Column("reason")]
    [MaxLength(20)]
    public ReportReason Reason { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("admin_note")]
    public string? AdminNote { get; set; }

    [Column("status")]
    [MaxLength(20)]
    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("resolved_at")]
    public DateTime? ResolvedAt { get; set; }

    [Column("resolved_by")]
    public Guid? ResolvedBy { get; set; }

    // Navigation Properties
    [ForeignKey("ReporterId")]
    public virtual User Reporter { get; set; } = null!;

    [ForeignKey("ResolvedBy")]
    public virtual User? Resolver { get; set; }
}

public enum ReportEntityType
{
    Course,
    Review,
    Comment,
    User,
    Message
}

public enum ReportReason
{
    Spam,
    Inappropriate,
    Copyright,
    Other,
    Harassment
}

public enum ReportStatus
{
    Pending,
    Dismissed,
    ActionTaken
}
