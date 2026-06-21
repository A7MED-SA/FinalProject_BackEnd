using System.Net;
using System.Net.Mail;
using Athary.Application.Interfaces.Authentication;
using Athary.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Athary.Infrastructure.Services.Communication;

public sealed class EmailService : IEmailService
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

    public async Task SendEmailVerificationAsync(string email, string name, string otp, CancellationToken cancellationToken = default)
    {
        var subject = "تفعيل الحساب - منصة آثاري";
        var body = GetEmailVerificationTemplate(name, otp);
        await SendEmailAsync(email, subject, body, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(string email, string name, string otp, CancellationToken cancellationToken = default)
    {
        var subject = "إعادة تعيين كلمة المرور - منصة آثاري";
        var body = GetPasswordResetTemplate(name, otp);
        await SendEmailAsync(email, subject, body, cancellationToken);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (!_emailSettings.IsEnabled)
        {
            _logger.LogWarning("Email service is disabled. Skipping email to {Email}", toEmail);
            return;
        }

        try
        {
#pragma warning disable CS0618
            using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
            {
                EnableSsl = _emailSettings.EnableSsl,
                Timeout = _emailSettings.TimeoutSeconds * 1000
            };

            if (!string.IsNullOrEmpty(_emailSettings.Username))
            {
                client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
            }
#pragma warning restore CS0618

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

            await client.SendMailAsync(mailMessage, cancellationToken);
            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw new Exception($"Failed to send email: {ex.Message}", ex);
        }
    }

    public async Task SendInstructorRequestApprovedAsync(string email, string name, string? adminNotes, CancellationToken cancellationToken = default)
    {
        var subject = "✅ تمت الموافقة على طلبك - منصة آثاري";
        var body = GetInstructorRequestApprovedTemplate(name, adminNotes);
        await SendEmailAsync(email, subject, body, cancellationToken);
    }

    public async Task SendInstructorRequestRejectedAsync(string email, string name, string? rejectionReason, string? adminNotes, CancellationToken cancellationToken = default)
    {
        var subject = "❌ تم رفض طلبك - منصة آثاري";
        var body = GetInstructorRequestRejectedTemplate(name, rejectionReason, adminNotes);
        await SendEmailAsync(email, subject, body, cancellationToken);
    }

    public async Task SendInstructorRequestMoreInfoAsync(string email, string name, string? adminNotes, CancellationToken cancellationToken = default)
    {
        var subject = "📋 نحتاج معلومات إضافية - منصة آثاري";
        var body = GetInstructorRequestMoreInfoTemplate(name, adminNotes);
        await SendEmailAsync(email, subject, body, cancellationToken);
    }

    private static string GetEmailVerificationTemplate(string name, string otp)
    {
        return $@"
    <!DOCTYPE html>
    <html lang='ar' dir='rtl'>
    <head>
        <meta charset='UTF-8'>
        <title>تأكيد الحساب</title>
    </head>
    <body style='margin:0;padding:0;background-color:#F4F8FA;font-family:Segoe UI,Arial,sans-serif;'>

        <table width='100%' cellpadding='0' cellspacing='0'>
            <tr>
                <td align='center' style='padding:40px 20px;'>

                    <table width='600' cellpadding='0' cellspacing='0' style='background:#ffffff;border-radius:16px;box-shadow:0 8px 30px rgba(0,0,0,0.08);overflow:hidden;'>

                        <tr>
                            <td style='background:#3F9AAE;padding:24px;text-align:center;color:#ffffff;font-size:22px;font-weight:700;'>
                                📚 منصة آثاري التعليمية
                            </td>
                        </tr>

                        <tr>
                            <td style='padding:40px;color:#1F2937;'>

                                <h2 style='margin-top:0;'>مرحباً {name} 👋</h2>

                                <p style='line-height:1.8;color:#4B5563;font-size:15px;'>
                                    سعداء بانضمامك إلى منصة <strong>آثاري</strong>.
                                    لتفعيل حسابك، استخدم رمز التحقق التالي:
                                </p>

                                <div style='margin:32px 0;padding:24px;text-align:center;border:2px dashed #3F9AAE;border-radius:12px;background:#F9FCFD;'>
                                    <span style='font-size:36px;font-weight:700;letter-spacing:8px;color:#3F9AAE;'>
                                        {otp}
                                    </span>
                                </div>

                                <p style='font-size:14px;color:#6B7280;'>
                                    ⏱️ الرمز صالح لمدة <strong>15 دقيقة</strong>.
                                </p>

                                <p style='margin-top:24px;font-size:13px;color:#F96E5B;'>
                                    ⚠️ إذا لم تقم بإنشاء هذا الحساب، يمكنك تجاهل هذه الرسالة بأمان.
                                </p>

                            </td>
                        </tr>

                        <tr>
                            <td style='background:#F9FAFB;padding:20px;text-align:center;font-size:12px;color:#9CA3AF;'>
                                © 2026 منصة آثاري التعليمية — جميع الحقوق محفوظة
                            </td>
                        </tr>

                    </table>

                </td>
            </tr>
        </table>

    </body>
    </html>";
    }

    private static string GetPasswordResetTemplate(string name, string otp)
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

    private static string GetInstructorRequestApprovedTemplate(string name, string? adminNotes)
    {
        return $@"
<!DOCTYPE html>
<html lang='ar' dir='rtl'>
<head>
    <meta charset='UTF-8'>
    <title>تمت الموافقة على طلبك</title>
</head>
<body style='margin:0;padding:0;background-color:#F4F8FA;font-family:Segoe UI,Arial,sans-serif;'>

    <table width='100%' cellpadding='0' cellspacing='0'>
        <tr>
            <td align='center' style='padding:40px 20px;'>

                <table width='600' cellpadding='0' cellspacing='0' style='background:#ffffff;border-radius:16px;box-shadow:0 8px 30px rgba(0,0,0,0.08);overflow:hidden;'>

                    <tr>
                        <td style='background:#10B981;padding:24px;text-align:center;color:#ffffff;font-size:22px;font-weight:700;'>
                            ✅ تم قبول طلبك
                        </td>
                    </tr>

                    <tr>
                        <td style='padding:40px;color:#1F2937;'>

                            <h2 style='margin-top:0;'>مرحباً {name} 👏</h2>

                            <p style='line-height:1.8;color:#4B5563;font-size:15px;'>
                                نود إعلامك بأنه تم <strong>الموافقة على طلبك</strong> لتصبح معلم في منصة آثاري التعليمية!
                            </p>

                            <div style='background:#ECFDF5;border-left:4px solid #10B981;padding:20px;margin:24px 0;border-radius:8px;'>
                                <p style='margin:0;color:#065F46;font-size:14px;'>
                                    🎉 مبروك! لقد أصبحت الآن جزءاً من فريق المعلمين في آثاري.
                                </p>
                            </div>

                            {(string.IsNullOrEmpty(adminNotes) ? "" : $@"
                            <div style='background:#F3F4F6;padding:20px;margin:24px 0;border-radius:8px;'>
                                <h3 style='margin-top:0;color:#374151;font-size:16px;'>ملاحظات الفريق:</h3>
                                <p style='margin:0;color:#4B5563;line-height:1.6;font-size:14px;'>
                                    {adminNotes}
                                </p>
                            </div>
                            ")}

                            <div style='background:#EFF6FF;padding:20px;margin:24px 0;border-radius:8px;'>
                                <h3 style='margin-top:0;color:#1E40AF;font-size:16px;'>ما الذي ينتظرك الآن؟</h3>
                                <ul style='margin:0;padding-right:20px;color:#374151;line-height:1.8;font-size:14px;'>
                                    <li>يمكنك الآن إنشاء كورساتك التعليمية</li>
                                    <li>إدارة طلابك ومتابعة تقدمهم</li>
                                    <li>التفاعل مع المجتمع التعليمي</li>
                                </ul>
                            </div>

                            <p style='margin-top:24px;font-size:13px;color:#6B7280;'>
                                🌟 نتطلع لمساهمتك القيمة في مجتمعنا التعليمي!
                            </p>

                        </td>
                    </tr>

                    <tr>
                        <td style='background:#F9FAFB;padding:20px;text-align:center;font-size:12px;color:#9CA3AF;'>
                            © 2026 منصة آثاري التعليمية — جميع الحقوق محفوظة
                        </td>
                    </tr>

                </table>

            </td>
        </tr>
    </table>

</body>
</html>";
    }

    private static string GetInstructorRequestRejectedTemplate(string name, string? rejectionReason, string? adminNotes)
    {
        return $@"
<!DOCTYPE html>
<html lang='ar' dir='rtl'>
<head>
    <meta charset='UTF-8'>
    <title>تم رفض طلبك</title>
</head>
<body style='margin:0;padding:0;background-color:#F4F8FA;font-family:Segoe UI,Arial,sans-serif;'>

    <table width='100%' cellpadding='0' cellspacing='0'>
        <tr>
            <td align='center' style='padding:40px 20px;'>

                <table width='600' cellpadding='0' cellspacing='0' style='background:#ffffff;border-radius:16px;box-shadow:0 8px 30px rgba(0,0,0,0.08);overflow:hidden;'>

                    <tr>
                        <td style='background:#F96E5B;padding:24px;text-align:center;color:#ffffff;font-size:22px;font-weight:700;'>
                            ❌ تم رفض طلبك
                        </td>
                    </tr>

                    <tr>
                        <td style='padding:40px;color:#1F2937;'>

                            <h2 style='margin-top:0;'>مرحباً {name} 👋</h2>

                            <p style='line-height:1.8;color:#4B5563;font-size:15px;'>
                                نود إعلامك بأنه تم <strong>رفض طلبك</strong> لتصبح معلم في منصة آثاري التعليمية.
                            </p>

                            <div style='background:#FEF2F2;border-left:4px solid #F96E5B;padding:20px;margin:24px 0;border-radius:8px;'>
                                <p style='margin:0;color:#B91C1C;font-size:14px;'>
                                    ⚠️ لا تقلق، يمكنك إعادة تقديم الطلب بعد معالجة الملاحظات أدناه.
                                </p>
                            </div>

                            {(string.IsNullOrEmpty(rejectionReason) ? "" : $@"
                            <div style='background:#FEE2E2;padding:20px;margin:24px 0;border-radius:8px;'>
                                <h3 style='margin-top:0;color:#DC2626;font-size:16px;'>سبب الرفض:</h3>
                                <p style='margin:0;color:#991B1B;line-height:1.6;font-size:14px;'>
                                    {rejectionReason}
                                </p>
                            </div>
                            ")}

                            {(string.IsNullOrEmpty(adminNotes) ? "" : $@"
                            <div style='background:#F3F4F6;padding:20px;margin:24px 0;border-radius:8px;'>
                                <h3 style='margin-top:0;color:#374151;font-size:16px;'>ملاحظات الفريق:</h3>
                                <p style='margin:0;color:#4B5563;line-height:1.6;font-size:14px;'>
                                    {adminNotes}
                                </p>
                            </div>
                            ")}

                            <div style='background:#EFF6FF;padding:20px;margin:24px 0;border-radius:8px;'>
                                <h3 style='margin-top:0;color:#1E40AF;font-size:16px;'>الخطوات التالية:</h3>
                                <ul style='margin:0;padding-right:20px;color:#374151;line-height:1.8;font-size:14px;'>
                                    <li>راجع المتطلبات للحصول على دور المعلم</li>
                                    <li>أرفق مستندات إضافية إذا لزم الأمر</li>
                                    <li>أعد تقديم طلبك بعد أسبوع على الأقل</li>
                                </ul>
                            </div>

                            <p style='margin-top:24px;font-size:13px;color:#6B7280;'>
                                🤝 نقدر اهتمامك ونتطلع لرؤيتك في المستقبل!
                            </p>

                        </td>
                    </tr>

                    <tr>
                        <td style='background:#F9FAFB;padding:20px;text-align:center;font-size:12px;color:#9CA3AF;'>
                            © 2026 منصة آثاري التعليمية — جميع الحقوق محفوظة
                        </td>
                    </tr>

                </table>

            </td>
        </tr>
    </table>

</body>
</html>";
    }

    private static string GetInstructorRequestMoreInfoTemplate(string name, string? adminNotes)
    {
        return $@"
<!DOCTYPE html>
<html lang='ar' dir='rtl'>
<head>
    <meta charset='UTF-8'>
    <title>نحتاج معلومات إضافية</title>
</head>
<body style='margin:0;padding:0;background-color:#F4F8FA;font-family:Segoe UI,Arial,sans-serif;'>

    <table width='100%' cellpadding='0' cellspacing='0'>
        <tr>
            <td align='center' style='padding:40px 20px;'>

                <table width='600' cellpadding='0' cellspacing='0' style='background:#ffffff;border-radius:16px;box-shadow:0 8px 30px rgba(0,0,0,0.08);overflow:hidden;'>

                    <tr>
                        <td style='background:#F59E0B;padding:24px;text-align:center;color:#ffffff;font-size:22px;font-weight:700;'>
                            📋 نحتاج معلومات إضافية
                        </td>
                    </tr>

                    <tr>
                        <td style='padding:40px;color:#1F2937;'>

                            <h2 style='margin-top:0;'>مرحباً {name} 👋</h2>

                            <p style='line-height:1.8;color:#4B5563;font-size:15px;'>
                                شكراً لتقديم طلبك لتصبح معلم في منصة آثاري التعليمية.
                            </p>

                            <p style='line-height:1.8;color:#4B5563;font-size:15px;'>
                                نود إعلامك بأن طلبك يحتاج إلى <strong>مزيد من المعلومات</strong> قبل اتخاذ القرار النهائي.
                            </p>

                            <div style='background:#FFF9DB;border-left:4px solid #F59E0B;padding:20px;margin:24px 0;border-radius:8px;'>
                                <p style='margin:0;color:#92400E;font-size:14px;'>
                                    ⏳ يرجى تحديث طلبك بالمعلومات المطلوبة في أقرب وقت ممكن.
                                </p>
                            </div>

                            {(string.IsNullOrEmpty(adminNotes) ? "" : $@"
                            <div style='background:#F3F4F6;padding:20px;margin:24px 0;border-radius:8px;'>
                                <h3 style='margin-top:0;color:#374151;font-size:16px;'>ما الذي نحتاجه منك:</h3>
                                <p style='margin:0;color:#4B5563;line-height:1.6;font-size:14px;'>
                                    {adminNotes}
                                </p>
                            </div>
                            ")}

                            <div style='background:#EFF6FF;padding:20px;margin:24px 0;border-radius:8px;'>
                                <h3 style='margin-top:0;color:#1E40AF;font-size:16px;'>كيفية التحديث:</h3>
                                <ul style='margin:0;padding-right:20px;color:#374151;line-height:1.8;font-size:14px;'>
                                    <li>سجّل الدخول إلى حسابك</li>
                                    <li>انتقل إلى صفحة طلبات المعلم</li>
                                    <li>قم بإرفاق المستندات أو المعلومات المطلوبة</li>
                                </ul>
                            </div>

                            <p style='margin-top:24px;font-size:13px;color:#6B7280;'>
                                📞 إذا كان لديك أي استفسار، لا تتردد في التواصل معنا.
                            </p>

                        </td>
                    </tr>

                    <tr>
                        <td style='background:#F9FAFB;padding:20px;text-align:center;font-size:12px;color:#9CA3AF;'>
                            © 2026 منصة آثاري التعليمية — جميع الحقوق محفوظة
                        </td>
                    </tr>

                </table>

            </td>
        </tr>
    </table>

</body>
</html>";
    }
}
