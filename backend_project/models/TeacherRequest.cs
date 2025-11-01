using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("teacher_requests")]
public class TeacherRequest : BaseEntity
{

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("status")]
    [MaxLength(50)]
    public TeacherRequestStatus Status { get; set; } = TeacherRequestStatus.Pending;

    [Column("message")]
    public string? Message { get; set; }

    [Column("admin_notes")]
    public string? AdminNotes { get; set; }

    [Column("rejection_reason")]
    public string? RejectionReason { get; set; }

    [Column("submitted_at")]
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    [Column("processed_at")]
    public DateTime? ProcessedAt { get; set; }

    [Column("processed_by")]
    public Guid? ProcessedBy { get; set; }

    // Navigation Properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("ProcessedBy")]
    public virtual User? ProcessedByUser { get; set; }

    public virtual ICollection<TeacherRequestDocument> Documents { get; set; } = new List<TeacherRequestDocument>();
}

public enum TeacherRequestStatus
{
    Pending,
    Approved,
    Rejected,
    RequiresMoreInfo
}
