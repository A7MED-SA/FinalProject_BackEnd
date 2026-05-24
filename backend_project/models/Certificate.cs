using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("certificates")]
public class Certificate : BaseEntity
{
    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("course_id")]
    public Guid CourseId { get; set; }

    [Required]
    [Column("enrollment_id")]
    public Guid EnrollmentId { get; set; }

    [Required]
    [Column("certificate_file_id")]
    public Guid CertificateFileId { get; set; }

    [Required, MaxLength(100)]
    [Column("verification_code")]
    public string VerificationCode { get; set; } = string.Empty;

    [Column("status")]
    [MaxLength(20)]
    public CertificateStatus Status { get; set; } = CertificateStatus.Valid;

    [Column("issued_at")]
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    [Column("completed_at")]
    public DateTime CompletedAt { get; set; }

    [Column("revoked_at")]
    public DateTime? RevokedAt { get; set; }

    [Column("revoked_by")]
    public Guid? RevokedBy { get; set; }

    [ForeignKey(nameof(CertificateFileId))]
    public UploadedFile CertificateFile { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(CourseId))]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey(nameof(EnrollmentId))]
    public virtual Enrollment Enrollment { get; set; } = null!;

    [ForeignKey(nameof(RevokedBy))]
    public virtual User? RevokedByUser { get; set; }
}

public enum CertificateStatus
{
    Valid,
    Revoked
}
