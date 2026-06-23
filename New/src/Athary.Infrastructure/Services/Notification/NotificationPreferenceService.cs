using Athary.Application.DTOs.Notification;
using Athary.Application.Interfaces.Notification;
using Athary.Domain.Entities;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Athary.Infrastructure.Services.Notification;

public class NotificationPreferenceService : INotificationPreferenceService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationPreferenceService> _logger;

    public NotificationPreferenceService(
        ApplicationDbContext context,
        ILogger<NotificationPreferenceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<NotificationPreferencesDto> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var preference = await _context.NotificationPreferences
            .FirstOrDefaultAsync(np => np.UserId == userId, cancellationToken);

        if (preference is null)
        {
            preference = new NotificationPreference
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.NotificationPreferences.Add(preference);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return MapToDto(preference);
    }

    public async Task<NotificationPreferencesDto> UpdateAsync(Guid userId, UpdateNotificationPreferencesDto dto, CancellationToken cancellationToken = default)
    {
        var preference = await _context.NotificationPreferences
            .FirstOrDefaultAsync(np => np.UserId == userId, cancellationToken);

        if (preference is null)
        {
            preference = new NotificationPreference
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.NotificationPreferences.Add(preference);
        }

        if (dto.EmailNotifications.HasValue)
            preference.EmailNotifications = dto.EmailNotifications.Value;

        if (dto.PushNotifications.HasValue)
            preference.PushNotifications = dto.PushNotifications.Value;

        if (dto.CourseUpdates.HasValue)
            preference.CourseUpdates = dto.CourseUpdates.Value;

        if (dto.MarketingEmails.HasValue)
            preference.MarketingEmails = dto.MarketingEmails.Value;

        if (dto.NewMessageAlerts.HasValue)
            preference.NewMessageAlerts = dto.NewMessageAlerts.Value;

        if (dto.LiveSessionReminders.HasValue)
            preference.LiveSessionReminders = dto.LiveSessionReminders.Value;

        if (dto.QuizReminders.HasValue)
            preference.QuizReminders = dto.QuizReminders.Value;

        if (dto.CertificateAchievements.HasValue)
            preference.CertificateAchievements = dto.CertificateAchievements.Value;

        if (dto.AnnouncementAlerts.HasValue)
            preference.AnnouncementAlerts = dto.AnnouncementAlerts.Value;

        preference.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Notification preferences updated for user {UserId}", userId);

        return MapToDto(preference);
    }

    private static NotificationPreferencesDto MapToDto(NotificationPreference preference)
    {
        return new NotificationPreferencesDto
        {
            EmailNotifications = preference.EmailNotifications,
            PushNotifications = preference.PushNotifications,
            CourseUpdates = preference.CourseUpdates,
            MarketingEmails = preference.MarketingEmails,
            NewMessageAlerts = preference.NewMessageAlerts,
            LiveSessionReminders = preference.LiveSessionReminders,
            QuizReminders = preference.QuizReminders,
            CertificateAchievements = preference.CertificateAchievements,
            AnnouncementAlerts = preference.AnnouncementAlerts
        };
    }
}
