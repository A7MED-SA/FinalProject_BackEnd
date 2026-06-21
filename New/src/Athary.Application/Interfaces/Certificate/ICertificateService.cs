using Athary.Application.DTOs.Certificate;

namespace Athary.Application.Interfaces.Certificate;

public interface ICertificateService
{
    Task<CertificateResponse> GenerateCertificateAsync(Guid enrollmentId, CancellationToken cancellationToken = default);
    Task<CertificateResponse> GetCertificateByIdAsync(Guid certificateId, Guid userId, CancellationToken cancellationToken = default);
    Task<List<CertificateResponse>> GetMyCertificatesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CertificateVerificationResponse> VerifyCertificateAsync(string verificationCode, CancellationToken cancellationToken = default);
    Task RevokeCertificateAsync(Guid certificateId, Guid adminId, string? reason = null, CancellationToken cancellationToken = default);
    Task<CertificateResponse> IssueCertificateManuallyAsync(Guid adminId, IssueCertificateRequest request, CancellationToken cancellationToken = default);
    Task<List<CertificateResponse>> GetAllCertificatesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Stream> GetCertificatePdfStreamAsync(Guid certificateId, Guid userId, CancellationToken cancellationToken = default);
}
