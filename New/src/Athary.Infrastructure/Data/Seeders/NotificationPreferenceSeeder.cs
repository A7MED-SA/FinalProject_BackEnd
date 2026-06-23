using Athary.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Data.Seeders;

public static class NotificationPreferenceSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var userIds = await context.Users
            .Where(u => u.DeletedAt == null)
            .Select(u => u.Id)
            .ToListAsync();

        if (userIds.Count == 0)
            return;

        var existingUserIds = await context.NotificationPreferences
            .Select(np => np.UserId)
            .ToListAsync();

        var newUserIds = userIds.Except(existingUserIds).ToList();

        if (newUserIds.Count == 0)
            return;

        var preferences = newUserIds.Select(userId => new NotificationPreference
        {
            UserId = userId,
            EmailNotifications = true,
            PushNotifications = true,
            CourseUpdates = true,
            MarketingEmails = false,
            NewMessageAlerts = true,
            LiveSessionReminders = true,
            QuizReminders = true,
            CertificateAchievements = true,
            AnnouncementAlerts = true,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        context.NotificationPreferences.AddRange(preferences);
        await context.SaveChangesAsync();
    }
}
