using backend_project.DTOs.Certificate;

namespace backend_project.Services.Interfaces;

public interface ICertificateService
{
    Task<CertificateResponse> GenerateCertificateAsync(Guid enrollmentId);
    Task<CertificateResponse> GetCertificateByIdAsync(Guid certificateId, Guid userId);
    Task<IEnumerable<CertificateResponse>> GetMyCertificatesAsync(Guid userId);
    Task<CertificateVerificationResponse> VerifyCertificateAsync(string verificationCode);
    Task RevokeCertificateAsync(Guid certificateId, Guid adminId, string? reason = null);
    Task<CertificateResponse> IssueCertificateManuallyAsync(Guid adminId, IssueCertificateRequest request);
    Task<IEnumerable<CertificateResponse>> GetAllCertificatesAsync(int page, int pageSize);
    Task<Stream> GetCertificatePdfStreamAsync(Guid certificateId, Guid userId);
}
