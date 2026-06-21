using Athary.Application.DTOs.Notification;
using Athary.Domain.Enums;

namespace Athary.Application.Interfaces.Notification;

public interface INotificationService
{
    Task<NotificationDto> CreateNotificationAsync(Guid userId, string title, string message, NotificationType type, string? linkUrl = null, string? icon = null, CancellationToken cancellationToken = default);
    Task<NotificationDto> CreateAndSendNotificationAsync(Guid userId, string title, string message, NotificationType type, string? linkUrl = null, string? icon = null, CancellationToken cancellationToken = default);
    Task<NotificationListDto> GetNotificationsAsync(Guid userId, int page = 1, int pageSize = 20, bool? isRead = null, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
    Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
    Task<int> DeleteAllNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
}
