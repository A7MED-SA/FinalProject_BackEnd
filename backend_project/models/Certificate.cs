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

    [Column("certificate_url")]
    [MaxLength(500)]
    public string? CertificateUrl { get; set; }

    [Required]
    [Column("verification_code")]
    [MaxLength(100)]
    public string VerificationCode { get; set; } = string.Empty;

    [Column("issued_at")]
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    // Navigation Properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("CourseId")]
    public virtual Course Course { get; set; } = null!;
}
