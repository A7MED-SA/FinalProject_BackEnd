using Athary.Application.DTOs.Certificate;
using Athary.Application.Interfaces.Authentication;
using Athary.Application.Interfaces.Certificate;
using Athary.Application.Interfaces.Notification;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Athary.Infrastructure.Services.Certificate;

public sealed class CertificateService : ICertificateService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IActivityLogService _activityLogService;

    public CertificateService(
        ApplicationDbContext context,
        INotificationService notificationService,
        IActivityLogService activityLogService)
    {
        _context = context;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<CertificateResponse> GenerateCertificateAsync(Guid enrollmentId, CancellationToken cancellationToken = default)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId, cancellationToken);

        if (enrollment == null)
            throw new KeyNotFoundException("Enrollment not found.");

        if (enrollment.Status != EnrollmentStatus.Completed)
            throw new InvalidOperationException("Enrollment must be completed to generate a certificate.");

        if (enrollment.CertificateId.HasValue)
        {
            var existing = await _context.Certificates.FindAsync(new object[] { enrollment.CertificateId.Value }, cancellationToken);
            if (existing != null)
                return MapToDto(existing);
        }

        var verificationCode = GenerateVerificationCode();
        var completedAt = enrollment.CompletedAt ?? DateTime.UtcNow;

        var certificate = new Athary.Domain.Entities.Certificate
        {
            UserId = enrollment.UserId,
            CourseId = enrollment.CourseId,
            EnrollmentId = enrollment.Id,
            VerificationCode = verificationCode,
            Status = CertificateStatus.Valid,
            IssuedAt = DateTime.UtcNow,
            CompletedAt = completedAt
        };

        _context.Certificates.Add(certificate);
        enrollment.CertificateId = certificate.Id;
        await _context.SaveChangesAsync(cancellationToken);

        await _activityLogService.LogActivityAsync(
            enrollment.UserId,
            "CertificateGenerated",
            $"Certificate issued for course '{enrollment.Course.Title}'",
            string.Empty,
            cancellationToken: cancellationToken);

        await _notificationService.CreateAndSendNotificationAsync(
            enrollment.UserId,
            "Certificate Issued",
            $"Congratulations! Your certificate for '{enrollment.Course.Title}' is now available.",
            NotificationType.Course,
            $"/certificates/{certificate.Id}",
            cancellationToken: cancellationToken);

        return MapToDto(certificate);
    }

    public async Task<CertificateResponse> GetCertificateByIdAsync(Guid certificateId, Guid userId, CancellationToken cancellationToken = default)
    {
        var certificate = await _context.Certificates
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.Id == certificateId, cancellationToken);

        if (certificate == null)
            throw new KeyNotFoundException("Certificate not found.");

        if (certificate.UserId != userId)
            throw new InvalidOperationException("Access denied. You can only view your own certificates.");

        return MapToDto(certificate);
    }

    public async Task<List<CertificateResponse>> GetMyCertificatesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var certificates = await _context.Certificates
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Course)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.IssuedAt)
            .ToListAsync(cancellationToken);

        return certificates.Select(MapToDto).ToList();
    }

    public async Task<CertificateVerificationResponse> VerifyCertificateAsync(string verificationCode, CancellationToken cancellationToken = default)
    {
        var certificate = await _context.Certificates
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.VerificationCode == verificationCode, cancellationToken);

        if (certificate == null)
        {
            return new CertificateVerificationResponse
            {
                IsValid = false,
                Status = "NotFound"
            };
        }

        return new CertificateVerificationResponse
        {
            IsValid = certificate.Status == CertificateStatus.Valid,
            FullName = $"{certificate.User.FirstName} {certificate.User.LastName}",
            CourseTitle = certificate.Course.Title,
            IssuedAt = certificate.IssuedAt,
            CompletedAt = certificate.CompletedAt,
            VerificationCode = certificate.VerificationCode,
            Status = certificate.Status.ToString()
        };
    }

    public async Task RevokeCertificateAsync(Guid certificateId, Guid adminId, string? reason = null, CancellationToken cancellationToken = default)
    {
        var certificate = await _context.Certificates
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.Id == certificateId, cancellationToken);

        if (certificate == null)
            throw new KeyNotFoundException("Certificate not found.");

        if (certificate.Status == CertificateStatus.Revoked)
            throw new InvalidOperationException("Certificate is already revoked.");

        certificate.Status = CertificateStatus.Revoked;
        certificate.RevokedAt = DateTime.UtcNow;
        certificate.RevokedBy = adminId;

        await _context.SaveChangesAsync(cancellationToken);

        await _activityLogService.LogActivityAsync(
            adminId,
            "CertificateRevoked",
            $"Certificate for course '{certificate.Course.Title}' revoked{(reason != null ? $": {reason}" : "")}",
            string.Empty,
            cancellationToken: cancellationToken);

        await _notificationService.CreateAndSendNotificationAsync(
            certificate.UserId,
            "Certificate Revoked",
            $"Your certificate for '{certificate.Course.Title}' has been revoked{(reason != null ? $": {reason}" : ".")}",
            NotificationType.System,
            cancellationToken: cancellationToken);
    }

    public async Task<CertificateResponse> IssueCertificateManuallyAsync(Guid adminId, IssueCertificateRequest request, CancellationToken cancellationToken = default)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == request.EnrollmentId, cancellationToken);

        if (enrollment == null)
            throw new KeyNotFoundException("Enrollment not found.");

        if (enrollment.CertificateId.HasValue)
        {
            var existing = await _context.Certificates.FindAsync(new object[] { enrollment.CertificateId.Value }, cancellationToken);
            if (existing != null)
                return MapToDto(existing);
        }

        var verificationCode = GenerateVerificationCode();
        var completedAt = enrollment.CompletedAt ?? DateTime.UtcNow;

        var certificate = new Athary.Domain.Entities.Certificate
        {
            UserId = request.UserId,
            CourseId = request.CourseId,
            EnrollmentId = request.EnrollmentId,
            VerificationCode = verificationCode,
            Status = CertificateStatus.Valid,
            IssuedAt = DateTime.UtcNow,
            CompletedAt = completedAt
        };

        _context.Certificates.Add(certificate);
        enrollment.CertificateId = certificate.Id;
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(certificate);
    }

    public async Task<List<CertificateResponse>> GetAllCertificatesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var certificates = await _context.Certificates
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Course)
            .OrderByDescending(c => c.IssuedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return certificates.Select(MapToDto).ToList();
    }

    public async Task<Stream> GetCertificatePdfStreamAsync(Guid certificateId, Guid userId, CancellationToken cancellationToken = default)
    {
        var certificate = await _context.Certificates
            .Include(c => c.User)
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.Id == certificateId, cancellationToken);

        if (certificate == null)
            throw new KeyNotFoundException("Certificate not found.");

        if (certificate.UserId != userId)
            throw new InvalidOperationException("Access denied. You can only download your own certificates.");

        var pdfBytes = GenerateCertificatePdf(
            $"{certificate.User.FirstName} {certificate.User.LastName}",
            certificate.Course.Title,
            certificate.CompletedAt,
            certificate.VerificationCode);

        return new MemoryStream(pdfBytes);
    }

    private static byte[] GenerateCertificatePdf(string fullName, string courseTitle, DateTime completedAt, string verificationCode)
    {
        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(50);
                page.PageColor(Colors.White);

                page.Header().Border(2).BorderColor(Colors.Blue.Medium).Padding(20).Column(col =>
                {
                    col.Item().AlignCenter().Text("ATHARY")
                        .FontSize(36).Bold().FontColor(Colors.Blue.Darken2);

                    col.Item().AlignCenter().Text("E-Learning Platform")
                        .FontSize(16).FontColor(Colors.Grey.Medium);

                    col.Item().PaddingTop(30).AlignCenter().Text("Certificate of Completion")
                        .FontSize(28).Bold().FontColor(Colors.Black);
                });

                page.Content().PaddingVertical(30).Column(col =>
                {
                    col.Item().AlignCenter().Text("This is to certify that")
                        .FontSize(18).FontColor(Colors.Grey.Darken2);

                    col.Item().PaddingTop(15).AlignCenter().Text(fullName)
                        .FontSize(32).Bold().FontColor(Colors.Blue.Darken2);

                    col.Item().PaddingTop(10).AlignCenter().Text("has successfully completed the course")
                        .FontSize(18).FontColor(Colors.Grey.Darken2);

                    col.Item().PaddingTop(10).AlignCenter().Text(courseTitle)
                        .FontSize(24).Bold().FontColor(Colors.Black);

                    col.Item().PaddingTop(10).AlignCenter().Text($"Completed on {completedAt:MMMM dd, yyyy}")
                        .FontSize(16).FontColor(Colors.Grey.Medium);
                });

                page.Footer().Column(col =>
                {
                    col.Item().PaddingTop(20).AlignCenter().Text($"Verification Code: {verificationCode}")
                        .FontSize(14).FontColor(Colors.Grey.Medium);

                    col.Item().PaddingTop(5).AlignCenter().Text($"Verify at: /api/certificates/verify/{verificationCode}")
                        .FontSize(12).FontColor(Colors.Blue.Medium);
                });
            });
        }).GeneratePdf();
    }

    private static string GenerateVerificationCode()
    {
        return "CERT-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }

    private static CertificateResponse MapToDto(Athary.Domain.Entities.Certificate certificate)
    {
        return new CertificateResponse
        {
            Id = certificate.Id,
            UserId = certificate.UserId,
            UserFullName = $"{certificate.User.FirstName} {certificate.User.LastName}",
            CourseId = certificate.CourseId,
            CourseTitle = certificate.Course.Title,
            VerificationCode = certificate.VerificationCode,
            Status = certificate.Status.ToString(),
            IssuedAt = certificate.IssuedAt,
            CompletedAt = certificate.CompletedAt,
            CertificateFileId = certificate.CertificateFileId,
            RevokedAt = certificate.RevokedAt
        };
    }
}
