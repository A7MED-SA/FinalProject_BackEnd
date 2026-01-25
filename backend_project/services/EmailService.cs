using backend_project.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace backend_project.Services;

/// <summary>
/// Production-ready email service with SMTP support
/// NO business logic - delivery only
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailVerificationAsync(string email, string name, string otp)
    {
        var subject = "تفعيل الحساب - منصة آثاري";
        var body = GetEmailVerificationTemplate(name, otp);
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(string email, string name, string otp)
    {
        var subject = "إعادة تعيين كلمة المرور - منصة آثاري";
        var body = GetPasswordResetTemplate(name, otp);
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        if (!_emailSettings.IsEnabled)
        {
            _logger.LogWarning("Email service is disabled. Skipping email to {Email}", toEmail);
            return;
        }

        try
        {
            using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = _emailSettings.EnableSsl,
                Timeout = _emailSettings.TimeoutSeconds * 1000
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            if (!string.IsNullOrEmpty(_emailSettings.ReplyToEmail))
            {
                mailMessage.ReplyToList.Add(new MailAddress(_emailSettings.ReplyToEmail));
            }

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw new Exception($"Failed to send email: {ex.Message}", ex);
        }
    }

    private string GetEmailVerificationTemplate(string name, string otp)
    {
        return $@"
<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; padding: 40px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ color: #3F9AAE; font-size: 28px; font-weight: bold; }}
        h1 {{ color: #333; font-size: 24px; }}
        .otp-box {{ background-color: #f8f9fa; border: 2px dashed #3F9AAE; border-radius: 8px; padding: 20px; text-align: center; margin: 30px 0; }}
        .otp {{ font-size: 36px; font-weight: bold; letter-spacing: 8px; color: #3F9AAE; }}
        .message {{ color: #666; line-height: 1.6; margin: 20px 0; }}
        .warning {{ color: #F96E5B; font-size: 14px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 40px; padding-top: 20px; border-top: 1px solid #eee; color: #999; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>📚 منصة آثاري التعليمية</div>
        </div>
        <h1>مرحباً {name}!</h1>
        <p class='message'>
            شكراً لتسجيلك في منصة آثاري التعليمية. لتفعيل حسابك، يرجى استخدام رمز التحقق التالي:
        </p>
        <div class='otp-box'>
            <div class='otp'>{otp}</div>
        </div>
        <p class='message'>
            هذا الرمز صالح لمدة 15 دقيقة فقط.
        </p>
        <p class='warning'>
            ⚠️ إذا لم تقم بإنشاء هذا الحساب، يرجى تجاهل هذه الرسالة.
        </p>
        <div class='footer'>
            © 2026 منصة آثاري التعليمية. جميع الحقوق محفوظة.
        </div>
    </div>
</body>
</html>";
    }

    private string GetPasswordResetTemplate(string name, string otp)
    {
        return $@"
<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; padding: 40px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ color: #3F9AAE; font-size: 28px; font-weight: bold; }}
        h1 {{ color: #333; font-size: 24px; }}
        .otp-box {{ background-color: #f8f9fa; border: 2px dashed #F96E5B; border-radius: 8px; padding: 20px; text-align: center; margin: 30px 0; }}
        .otp {{ font-size: 36px; font-weight: bold; letter-spacing: 8px; color: #F96E5B; }}
        .message {{ color: #666; line-height: 1.6; margin: 20px 0; }}
        .warning {{ color: #F96E5B; font-size: 14px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 40px; padding-top: 20px; border-top: 1px solid #eee; color: #999; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>📚 منصة آثاري التعليمية</div>
        </div>
        <h1>إعادة تعيين كلمة المرور</h1>
        <p class='message'>
            مرحباً {name}، لقد تلقينا طلباً لإعادة تعيين كلمة المرور الخاصة بك. يرجى استخدام الرمز التالي:
        </p>
        <div class='otp-box'>
            <div class='otp'>{otp}</div>
        </div>
        <p class='message'>
            هذا الرمز صالح لمدة 15 دقيقة فقط ويمكن استخدامه مرة واحدة فقط.
        </p>
        <p class='warning'>
            🔒 إذا لم تطلب إعادة تعيين كلمة المرور، يرجى تجاهل هذه الرسالة وحسابك آمن.
        </p>
        <div class='footer'>
            © 2026 منصة آثاري التعليمية. جميع الحقوق محفوظة.
        </div>
    </div>
</body>
</html>";
    }
}
