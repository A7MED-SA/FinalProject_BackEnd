using backend_project.DTOs.Email;

namespace backend_project.Services;

public interface IEmailService
{
    // Basic email sending
    Task<bool> SendEmailAsync(EmailMessageDto emailMessage);
    Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
    
    // Template-based emails
    Task<bool> SendTemplateEmailAsync(string toEmail, string templateName, Dictionary<string, object> parameters);
    Task<bool> SendWelcomeEmailAsync(string toEmail, string userName);
    Task<bool> SendEmailConfirmationAsync(string toEmail, string userName, string confirmationLink);
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink);
    Task<bool> SendPasswordChangedNotificationAsync(string toEmail, string userName);
    
    // Bulk emails
    Task<bool> SendBulkEmailAsync(List<string> toEmails, string subject, string body, bool isHtml = true);
    
    // Validation
    bool ValidateEmailConfiguration();
}