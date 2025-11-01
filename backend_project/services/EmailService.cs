using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using backend_project.Configuration;
using backend_project.DTOs.Email;

namespace backend_project.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(EmailMessageDto emailMessage)
    {
        if (!_emailSettings.IsEnabled)
        {
            _logger.LogInformation("Email service is disabled. Skipping email send.");
            return true; // Return true to not break workflows
        }

        if (!ValidateEmailConfiguration())
        {
            _logger.LogError("Email configuration is invalid");
            return false;
        }

        try
        {
            using var smtpClient = CreateSmtpClient();
            using var mailMessage = CreateMailMessage(emailMessage);

            await smtpClient.SendMailAsync(mailMessage);
            
            _logger.LogInformation("Email sent successfully to {Recipients}", 
                string.Join(", ", emailMessage.ToEmails));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipients}", 
                string.Join(", ", emailMessage.ToEmails));
            return false;
        }
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
    {
        var emailMessage = new EmailMessageDto
        {
            ToEmails = new List<string> { toEmail },
            Subject = subject,
            Body = body,
            IsHtml = isHtml
        };

        return await SendEmailAsync(emailMessage);
    }

    public async Task<bool> SendTemplateEmailAsync(string toEmail, string templateName, Dictionary<string, object> parameters)
    {
        var (subject, body) = GenerateEmailFromTemplate(templateName, parameters);
        return await SendEmailAsync(toEmail, subject, body);
    }

    public async Task<bool> SendWelcomeEmailAsync(string toEmail, string userName)
    {
        var parameters = new Dictionary<string, object>
        {
            { "UserName", userName },
            { "Year", DateTime.Now.Year }
        };

        return await SendTemplateEmailAsync(toEmail, "welcome", parameters);
    }

    public async Task<bool> SendEmailConfirmationAsync(string toEmail, string userName, string confirmationLink)
    {
        var parameters = new Dictionary<string, object>
        {
            { "UserName", userName },
            { "ConfirmationLink", confirmationLink },
            { "ExpirationHours", 24 }
        };

        return await SendTemplateEmailAsync(toEmail, "email_confirmation", parameters);
    }

    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink)
    {
        var parameters = new Dictionary<string, object>
        {
            { "UserName", userName },
            { "ResetLink", resetLink },
            { "ExpirationMinutes", 30 }
        };

        return await SendTemplateEmailAsync(toEmail, "password_reset", parameters);
    }

    public async Task<bool> SendPasswordChangedNotificationAsync(string toEmail, string userName)
    {
        var parameters = new Dictionary<string, object>
        {
            { "UserName", userName },
            { "ChangeDateTime", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm UTC") }
        };

        return await SendTemplateEmailAsync(toEmail, "password_changed", parameters);
    }

    public async Task<bool> SendBulkEmailAsync(List<string> toEmails, string subject, string body, bool isHtml = true)
    {
        var tasks = toEmails.Select(email => SendEmailAsync(email, subject, body, isHtml));
        var results = await Task.WhenAll(tasks);
        return results.All(result => result);
    }

    public bool ValidateEmailConfiguration()
    {
        return !string.IsNullOrEmpty(_emailSettings.SmtpHost) &&
               !string.IsNullOrEmpty(_emailSettings.Username) &&
               !string.IsNullOrEmpty(_emailSettings.Password) &&
               !string.IsNullOrEmpty(_emailSettings.FromEmail);
    }

    private SmtpClient CreateSmtpClient()
    {
        var smtpClient = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
        {
            Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
            EnableSsl = _emailSettings.EnableSsl,
            Timeout = _emailSettings.TimeoutSeconds * 1000
        };

        return smtpClient;
    }

    private MailMessage CreateMailMessage(EmailMessageDto emailMessage)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
            Subject = emailMessage.Subject,
            Body = emailMessage.Body,
            IsBodyHtml = emailMessage.IsHtml
        };

        // Add recipients
        foreach (var email in emailMessage.ToEmails)
        {
            mailMessage.To.Add(email);
        }

        foreach (var email in emailMessage.CcEmails)
        {
            mailMessage.CC.Add(email);
        }

        foreach (var email in emailMessage.BccEmails)
        {
            mailMessage.Bcc.Add(email);
        }

        // Add reply-to if configured
        if (!string.IsNullOrEmpty(_emailSettings.ReplyToEmail))
        {
            mailMessage.ReplyToList.Add(_emailSettings.ReplyToEmail);
        }

        // Add attachments
        foreach (var attachment in emailMessage.Attachments)
        {
            var stream = new MemoryStream(attachment.Content);
            var mailAttachment = new Attachment(stream, attachment.FileName, attachment.ContentType);
            mailMessage.Attachments.Add(mailAttachment);
        }

        return mailMessage;
    }

    private (string Subject, string Body) GenerateEmailFromTemplate(string templateName, Dictionary<string, object> parameters)
    {
        // Simple template system - في الواقع يمكن استخدام Razor أو أي template engine
        return templateName.ToLower() switch
        {
            "welcome" => GenerateWelcomeTemplate(parameters),
            "email_confirmation" => GenerateEmailConfirmationTemplate(parameters),
            "password_reset" => GeneratePasswordResetTemplate(parameters),
            "password_changed" => GeneratePasswordChangedTemplate(parameters),
            _ => throw new ArgumentException($"Unknown template: {templateName}")
        };
    }

    private (string Subject, string Body) GenerateWelcomeTemplate(Dictionary<string, object> parameters)
    {
        var userName = parameters.GetValueOrDefault("UserName", "User").ToString();
        var year = parameters.GetValueOrDefault("Year", DateTime.Now.Year).ToString();

        var subject = "مرحباً بك في منصة آثاري التعليمية!";
        var body = $@"
            <html>
            <body dir='rtl' style='font-family: Arial, sans-serif;'>
                <h2>مرحباً {userName}!</h2>
                <p>أهلاً وسهلاً بك في منصة آثاري التعليمية.</p>
                <p>نحن سعداء بانضمامك إلينا ونتطلع لرحلة تعليمية مميزة معك.</p>
                <br>
                <p>فريق آثاري</p>
                <hr>
                <small>© {year} منصة آثاري التعليمية. جميع الحقوق محفوظة.</small>
            </body>
            </html>";

        return (subject, body);
    }

    private (string Subject, string Body) GenerateEmailConfirmationTemplate(Dictionary<string, object> parameters)
    {
        var userName = parameters.GetValueOrDefault("UserName", "User").ToString();
        var confirmationLink = parameters.GetValueOrDefault("ConfirmationLink", "").ToString();
        var expirationHours = parameters.GetValueOrDefault("ExpirationHours", "24").ToString();

        var subject = "تأكيد البريد الإلكتروني - منصة آثاري";
        var body = $@"
            <html>
            <body dir='rtl' style='font-family: Arial, sans-serif;'>
                <h2>مرحباً {userName}</h2>
                <p>شكراً لك على التسجيل في منصة آثاري التعليمية.</p>
                <p>يرجى النقر على الرابط أدناه لتأكيد بريدك الإلكتروني:</p>
                <br>
                <a href='{confirmationLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                    تأكيد البريد الإلكتروني
                </a>
                <br><br>
                <p><strong>ملاحظة:</strong> هذا الرابط صالح لمدة {expirationHours} ساعة فقط.</p>
                <br>
                <p>فريق آثاري</p>
            </body>
            </html>";

        return (subject, body);
    }

    private (string Subject, string Body) GeneratePasswordResetTemplate(Dictionary<string, object> parameters)
    {
        var userName = parameters.GetValueOrDefault("UserName", "User").ToString();
        var resetLink = parameters.GetValueOrDefault("ResetLink", "").ToString();
        var expirationMinutes = parameters.GetValueOrDefault("ExpirationMinutes", "30").ToString();

        var subject = "إعادة تعيين كلمة المرور - منصة آثاري";
        var body = $@"
            <html>
            <body dir='rtl' style='font-family: Arial, sans-serif;'>
                <h2>مرحباً {userName}</h2>
                <p>لقد طلبت إعادة تعيين كلمة المرور لحسابك في منصة آثاري.</p>
                <p>يرجى النقر على الرابط أدناه لإعادة تعيين كلمة المرور:</p>
                <br>
                <a href='{resetLink}' style='background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                    إعادة تعيين كلمة المرور
                </a>
                <br><br>
                <p><strong>ملاحظة:</strong> هذا الرابط صالح لمدة {expirationMinutes} دقيقة فقط.</p>
                <p>إذا لم تطلب إعادة تعيين كلمة المرور، يرجى تجاهل هذه الرسالة.</p>
                <br>
                <p>فريق آثاري</p>
            </body>
            </html>";

        return (subject, body);
    }

    private (string Subject, string Body) GeneratePasswordChangedTemplate(Dictionary<string, object> parameters)
    {
        var userName = parameters.GetValueOrDefault("UserName", "User").ToString();
        var changeDateTime = parameters.GetValueOrDefault("ChangeDateTime", DateTime.UtcNow.ToString()).ToString();

        var subject = "تم تغيير كلمة المرور - منصة آثاري";
        var body = $@"
            <html>
            <body dir='rtl' style='font-family: Arial, sans-serif;'>
                <h2>مرحباً {userName}</h2>
                <p>نود إعلامك أنه تم تغيير كلمة المرور لحسابك في منصة آثاري بنجاح.</p>
                <p><strong>تاريخ ووقت التغيير:</strong> {changeDateTime}</p>
                <br>
                <p>إذا لم تقم بهذا التغيير، يرجى التواصل معنا فوراً.</p>
                <br>
                <p>فريق آثاري</p>
            </body>
            </html>";

        return (subject, body);
    }
}