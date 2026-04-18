namespace backend_project.Services;

public interface IEmailService
{
    /// <summary>
    /// Sends email verification OTP to user
    /// </summary>
    Task SendEmailVerificationAsync(string email, string name, string otp);

    /// <summary>
    /// Sends password reset OTP to user
    /// </summary>
    Task SendPasswordResetEmailAsync(string email, string name, string otp);

    /// <summary>
    /// Generic email sending method
    /// </summary>
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    Task SendTeacherRequestApprovedAsync(string email, string name, string? adminNotes);
    Task SendTeacherRequestRejectedAsync(string email, string name, string? rejectionReason, string? adminNotes);
    Task SendTeacherRequestMoreInfoAsync(string email, string name, string? adminNotes);
}
