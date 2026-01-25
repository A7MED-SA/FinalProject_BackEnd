using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("enrollments")]
public class Enrollment : BaseEntity
{

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("course_id")]
    public Guid CourseId { get; set; }

    [Column("enrolled_at")]
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    [Column("status")]
    [MaxLength(20)]
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.InProgress;

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [Column("last_accessed_at")]
    public DateTime? LastAccessedAt { get; set; }

    [Column("progress_percentage", TypeName = "decimal(5,2)")]
    public decimal ProgressPercentage { get; set; } = 0;

    [Column("certificate_id")]
    public Guid? CertificateId { get; set; }

    [Column("source")]
    [MaxLength(20)]
    public EnrollmentSource Source { get; set; } = EnrollmentSource.Purchase;

    [Column("access_expires_at")]
    public DateTime? AccessExpiresAt { get; set; }

    [Column("is_refunded")]
    public bool IsRefunded { get; set; } = false;

    // Navigation Properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("CourseId")]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey("CertificateId")]
    public virtual Certificate? Certificate { get; set; }

    public virtual ICollection<ContentProgress> ContentProgresses { get; set; } = new List<ContentProgress>();
    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
}

public enum EnrollmentStatus
{
    InProgress,
    Completed,
    Expired,
    Refunded
}

public enum EnrollmentSource
{
    Purchase,
    Gift,
    AdminGrant,
    Coupon
}
