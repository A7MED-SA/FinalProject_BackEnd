using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("reviews")]
public class Review : BaseEntity
{

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("course_id")]
    public Guid CourseId { get; set; }

    [Required]
    [Range(1, 5)]
    [Column("rating")]
    public int Rating { get; set; }

    [Column("comment")]
    public string? Comment { get; set; }

    [Column("status")]
    [MaxLength(20)]
    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;

    [Column("is_verified")]
    public bool IsVerified { get; set; } = false;

    [Column("helpful_count")]
    public int HelpfulCount { get; set; } = 0;

    [Column("not_helpful_count")]
    public int NotHelpfulCount { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("moderated_at")]
    public DateTime? ModeratedAt { get; set; }

    [Column("moderated_by")]
    public Guid? ModeratedBy { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    // Navigation Properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("CourseId")]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey("ModeratedBy")]
    public virtual User? Moderator { get; set; }

    public virtual ICollection<ReviewHelpful> ReviewHelpfuls { get; set; } = new List<ReviewHelpful>();
}

public enum ReviewStatus
{
    Pending,
    Approved,
    Rejected
}
