using backend_project.DTOs.Notifications;
using backend_project.Models; 

namespace backend_project.Services.Notifications;

public interface INotificationService
{
    // إنشاء إشعار جديد
    Task<NotificationDto> CreateNotificationAsync(Guid userId, string title, string message, NotificationType type, string? linkUrl = null, string? icon = null);

    // إنشاء إشعار مع إرسال عبر SignalR
    Task<NotificationDto> CreateAndSendNotificationAsync(Guid userId, string title, string message, NotificationType type, string? linkUrl = null, string? icon = null);

    // الحصول على جميع الإشعارات لمستخدم
    Task<NotificationListDto> GetNotificationsAsync(Guid userId, int page = 1, int pageSize = 20, bool? isRead = null);

    // وضع علامة كمقروء
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);

    // وضع علامة على جميع الإشعارات كمقروءة
    Task<int> MarkAllAsReadAsync(Guid userId);

    // حذف إشعار
    Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId);

    // حذف جميع الإشعارات
    Task<int> DeleteAllNotificationsAsync(Guid userId);

    // الحصول على عدد الإشعارات غير المقروءة
    Task<int> GetUnreadCountAsync(Guid userId);
}