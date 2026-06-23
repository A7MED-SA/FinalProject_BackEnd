using Athary.Application.DTOs.Notification;

namespace Athary.Application.Interfaces.Notification;

public interface INotificationPreferenceService
{
    Task<NotificationPreferencesDto> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<NotificationPreferencesDto> UpdateAsync(Guid userId, UpdateNotificationPreferencesDto dto, CancellationToken cancellationToken = default);
}
