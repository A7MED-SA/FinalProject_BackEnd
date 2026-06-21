using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class Certificate : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public Guid EnrollmentId { get; set; }
    public Guid? CertificateFileId { get; set; }
    public UploadedFile? CertificateFile { get; set; }
    public string VerificationCode { get; set; } = string.Empty;
    public CertificateStatus Status { get; set; } = CertificateStatus.Valid;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime CompletedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public Guid? RevokedBy { get; set; }

    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public Enrollment Enrollment { get; set; } = null!;
    public User? RevokedByUser { get; set; }
}
