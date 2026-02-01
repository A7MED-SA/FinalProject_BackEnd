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
    [Column("certificate_file_id")]
    public Guid CertificateFileId { get; set; }

    [Required, MaxLength(100)]
    [Column("verification_code")]
    public string VerificationCode { get; set; } = string.Empty;

    [Column("issued_at")]
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CertificateFileId))]
    public UploadedFile CertificateFile { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(CourseId))]
    public virtual Course Course { get; set; } = null!;
}
