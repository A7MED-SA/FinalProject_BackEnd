using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("course_edit_requests")]
public class CourseEditRequest : BaseEntity
{
    [Required]
    [Column("course_id")]
    public Guid CourseId { get; set; }

    [Required]
    [Column("requested_by")]
    public Guid RequestedBy { get; set; }

    [Required]
    [Column("request_type")]
    public EditRequestType RequestType { get; set; }

    [Column("target_section_id")]
    public Guid? TargetSectionId { get; set; }

    [Column("target_item_id")]
    public Guid? TargetItemId { get; set; }

    [Required]
    [Column("operation")]
    public EditOperation Operation { get; set; }

    [Column("json_payload")]
    public string? JsonPayload { get; set; }

    [Required]
    [Column("status")]
    public EditRequestStatus Status { get; set; } = EditRequestStatus.Pending;

    [Column("admin_notes")]
    [MaxLength(1000)]
    public string? AdminNotes { get; set; }

    [Column("reviewed_at")]
    public DateTime? ReviewedAt { get; set; }

    [Column("reviewed_by")]
    public Guid? ReviewedBy { get; set; }

    [Column("is_emergency")]
    public bool IsEmergency { get; set; } = false;

    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    [Column("requested_at")]
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("CourseId")]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey("RequestedBy")]
    public virtual User RequestedByUser { get; set; } = null!;

    [ForeignKey("ReviewedBy")]
    public virtual User? ReviewedByUser { get; set; }
}

public enum EditRequestType
{
    Section = 0,
    SectionItem = 1,
    CourseProperty = 2
}

public enum EditOperation
{
    Create = 0,
    Update = 1,
    Delete = 2
}

public enum EditRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3,
    Expired = 4
}
