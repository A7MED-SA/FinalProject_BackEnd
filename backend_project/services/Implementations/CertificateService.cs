using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using backend_project.Data;
using backend_project.DTOs.Certificate;
using backend_project.Models;
using backend_project.Services.Interfaces;
using backend_project.Services.Notifications;

namespace backend_project.Services.Implementations;

public class CertificateService : ICertificateService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IWebHostEnvironment _environment;

    public CertificateService(
        ApplicationDbContext context,
        INotificationService notificationService,
        IWebHostEnvironment environment)
    {
        _context = context;
        _notificationService = notificationService;
        _environment = environment;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<CertificateResponse> GenerateCertificateAsync(Guid enrollmentId)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId);

        if (enrollment == null)
            throw new KeyNotFoundException("Enrollment not found.");

        if (enrollment.Status != EnrollmentStatus.Completed)
            throw new InvalidOperationException("Enrollment must be completed to generate a certificate.");

        if (enrollment.CertificateId.HasValue)
        {
            var existing = await _context.Certificates.FindAsync(enrollment.CertificateId.Value);
            if (existing != null)
                return MapToDto(existing);
        }

        var verificationCode = GenerateVerificationCode();
        var fullName = $"{enrollment.User.FirstName} {enrollment.User.LastName}";
        var completedAt = enrollment.CompletedAt ?? DateTime.UtcNow;

        var pdfBytes = GenerateCertificatePdf(fullName, enrollment.Course.Title, completedAt, verificationCode);

        var uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "certificates");
        Directory.CreateDirectory(uploadsPath);
        var filePath = Path.Combine(uploadsPath, $"{verificationCode}.pdf");
        await File.WriteAllBytesAsync(filePath, pdfBytes);

        var file = new UploadedFile
        {
            UploadedBy = enrollment.UserId,
            FilePath = $"/uploads/certificates/{verificationCode}.pdf",
            FileName = $"Certificate_{verificationCode}.pdf",
            OriginalName = $"Certificate_{enrollment.Course.Title}.pdf",
            FileType = StoredFileType.Certificate,
            Bucket = "certificates",
            Status = FileStatus.Ready,
            MimeType = "application/pdf",
            SizeBytes = pdfBytes.Length,
            Visibility = FileVisibility.Public,
            StorageProvider = "local",
            UploadedAt = DateTime.UtcNow
        };

        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        var certificate = new Certificate
        {
            UserId = enrollment.UserId,
            CourseId = enrollment.CourseId,
            EnrollmentId = enrollment.Id,
            CertificateFileId = file.Id,
            VerificationCode = verificationCode,
            Status = CertificateStatus.Valid,
            IssuedAt = DateTime.UtcNow,
            CompletedAt = completedAt
        };

        _context.Certificates.Add(certificate);
        enrollment.CertificateId = certificate.Id;
        await _context.SaveChangesAsync();

        await _notificationService.CreateAndSendNotificationAsync(
            enrollment.UserId,
            "Certificate Issued",
            $"Congratulations! Your certificate for '{enrollment.Course.Title}' is now available.",
            NotificationType.Course,
            $"/certificates/{certificate.Id}");

        return MapToDto(certificate);
    }

    public async Task<CertificateResponse> GetCertificateByIdAsync(Guid certificateId, Guid userId)
    {
        var certificate = await _context.Certificates
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.Id == certificateId);

        if (certificate == null)
            throw new KeyNotFoundException("Certificate not found.");

        if (certificate.UserId != userId)
            throw new InvalidOperationException("Access denied. You can only view your own certificates.");

        return MapToDto(certificate);
    }

    public async Task<IEnumerable<CertificateResponse>> GetMyCertificatesAsync(Guid userId)
    {
        var certificates = await _context.Certificates
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Course)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.IssuedAt)
            .ToListAsync();

        return certificates.Select(MapToDto);
    }

    public async Task<CertificateVerificationResponse> VerifyCertificateAsync(string verificationCode)
    {
        var certificate = await _context.Certificates
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.VerificationCode == verificationCode);

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

    public async Task RevokeCertificateAsync(Guid certificateId, Guid adminId, string? reason = null)
    {
        var certificate = await _context.Certificates
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.Id == certificateId);

        if (certificate == null)
            throw new KeyNotFoundException("Certificate not found.");

        if (certificate.Status == CertificateStatus.Revoked)
            throw new InvalidOperationException("Certificate is already revoked.");

        certificate.Status = CertificateStatus.Revoked;
        certificate.RevokedAt = DateTime.UtcNow;
        certificate.RevokedBy = adminId;

        await _context.SaveChangesAsync();

        await _notificationService.CreateAndSendNotificationAsync(
            certificate.UserId,
            "Certificate Revoked",
            $"Your certificate for '{certificate.Course.Title}' has been revoked{(reason != null ? $": {reason}" : ".")}",
            NotificationType.System);
    }

    public async Task<CertificateResponse> IssueCertificateManuallyAsync(Guid adminId, IssueCertificateRequest request)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == request.EnrollmentId);

        if (enrollment == null)
            throw new KeyNotFoundException("Enrollment not found.");

        if (enrollment.CertificateId.HasValue)
        {
            var existing = await _context.Certificates.FindAsync(enrollment.CertificateId.Value);
            if (existing != null)
                return MapToDto(existing);
        }

        var verificationCode = GenerateVerificationCode();
        var fullName = $"{enrollment.User.FirstName} {enrollment.User.LastName}";
        var completedAt = enrollment.CompletedAt ?? DateTime.UtcNow;

        var pdfBytes = GenerateCertificatePdf(fullName, enrollment.Course.Title, completedAt, verificationCode);

        var uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "certificates");
        Directory.CreateDirectory(uploadsPath);
        var filePath = Path.Combine(uploadsPath, $"{verificationCode}.pdf");
        await File.WriteAllBytesAsync(filePath, pdfBytes);

        var file = new UploadedFile
        {
            UploadedBy = request.UserId,
            FilePath = $"/uploads/certificates/{verificationCode}.pdf",
            FileName = $"Certificate_{verificationCode}.pdf",
            OriginalName = $"Certificate_{enrollment.Course.Title}.pdf",
            FileType = StoredFileType.Certificate,
            Bucket = "certificates",
            Status = FileStatus.Ready,
            MimeType = "application/pdf",
            SizeBytes = pdfBytes.Length,
            Visibility = FileVisibility.Public,
            StorageProvider = "local",
            UploadedAt = DateTime.UtcNow
        };

        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        var certificate = new Certificate
        {
            UserId = request.UserId,
            CourseId = request.CourseId,
            EnrollmentId = request.EnrollmentId,
            CertificateFileId = file.Id,
            VerificationCode = verificationCode,
            Status = CertificateStatus.Valid,
            IssuedAt = DateTime.UtcNow,
            CompletedAt = completedAt
        };

        _context.Certificates.Add(certificate);
        enrollment.CertificateId = certificate.Id;
        await _context.SaveChangesAsync();

        return MapToDto(certificate);
    }

    public async Task<IEnumerable<CertificateResponse>> GetAllCertificatesAsync(int page, int pageSize)
    {
        var certificates = await _context.Certificates
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Course)
            .OrderByDescending(c => c.IssuedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return certificates.Select(MapToDto);
    }

    public async Task<Stream> GetCertificatePdfStreamAsync(Guid certificateId, Guid userId)
    {
        var certificate = await _context.Certificates
            .Include(c => c.User)
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.Id == certificateId);

        if (certificate == null)
            throw new KeyNotFoundException("Certificate not found.");

        if (certificate.UserId != userId)
            throw new InvalidOperationException("Access denied. You can only download your own certificates.");

        var fullPath = Path.Combine(
            _environment.WebRootPath ?? _environment.ContentRootPath,
            certificate.CertificateFile.FilePath.TrimStart('/'));

        if (!File.Exists(fullPath))
        {
            var pdfBytes = GenerateCertificatePdf(
                $"{certificate.User.FirstName} {certificate.User.LastName}",
                certificate.Course.Title,
                certificate.CompletedAt,
                certificate.VerificationCode);

            return new MemoryStream(pdfBytes);
        }

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
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

    private static CertificateResponse MapToDto(Certificate certificate)
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
