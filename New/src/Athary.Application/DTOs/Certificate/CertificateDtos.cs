namespace Athary.Application.DTOs.Certificate;

public class CertificateResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public Guid? CertificateFileId { get; set; }
    public DateTime? RevokedAt { get; set; }
}

public class CertificateVerificationResponse
{
    public bool IsValid { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public string VerificationCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class IssueCertificateRequest
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public Guid EnrollmentId { get; set; }
}

public class RevokeCertificateRequest
{
    public string? Reason { get; set; }
}
