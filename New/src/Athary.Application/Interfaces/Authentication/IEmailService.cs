namespace Athary.Application.Interfaces.Authentication;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string name, string otp, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string email, string name, string otp, CancellationToken cancellationToken = default);
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
    Task SendInstructorRequestApprovedAsync(string email, string name, string? adminNotes, CancellationToken cancellationToken = default);
    Task SendInstructorRequestRejectedAsync(string email, string name, string? rejectionReason, string? adminNotes, CancellationToken cancellationToken = default);
    Task SendInstructorRequestMoreInfoAsync(string email, string name, string? adminNotes, CancellationToken cancellationToken = default);
}
