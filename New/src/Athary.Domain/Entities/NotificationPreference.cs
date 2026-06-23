namespace Athary.Domain.Entities;

public class NotificationPreference : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public bool EmailNotifications { get; set; } = true;

    public bool PushNotifications { get; set; } = true;

    public bool CourseUpdates { get; set; } = true;

    public bool MarketingEmails { get; set; } = false;

    public bool NewMessageAlerts { get; set; } = true;

    public bool LiveSessionReminders { get; set; } = true;

    public bool QuizReminders { get; set; } = true;

    public bool CertificateAchievements { get; set; } = true;

    public bool AnnouncementAlerts { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
