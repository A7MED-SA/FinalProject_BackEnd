namespace Athary.Application.DTOs.Notification;

public sealed record NotificationPreferencesDto
{
    public bool EmailNotifications { get; init; } = true;
    public bool PushNotifications { get; init; } = true;
    public bool CourseUpdates { get; init; } = true;
    public bool MarketingEmails { get; init; } = false;
    public bool NewMessageAlerts { get; init; } = true;
    public bool LiveSessionReminders { get; init; } = true;
    public bool QuizReminders { get; init; } = true;
    public bool CertificateAchievements { get; init; } = true;
    public bool AnnouncementAlerts { get; init; } = true;
}

public sealed record UpdateNotificationPreferencesDto
{
    public bool? EmailNotifications { get; init; }
    public bool? PushNotifications { get; init; }
    public bool? CourseUpdates { get; init; }
    public bool? MarketingEmails { get; init; }
    public bool? NewMessageAlerts { get; init; }
    public bool? LiveSessionReminders { get; init; }
    public bool? QuizReminders { get; init; }
    public bool? CertificateAchievements { get; init; }
    public bool? AnnouncementAlerts { get; init; }
}
