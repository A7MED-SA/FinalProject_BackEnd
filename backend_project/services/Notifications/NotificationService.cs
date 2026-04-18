using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.Notifications;
using backend_project.Models;
using backend_project.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace backend_project.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    // ========================================
    // إنشاء إشعار جديد
    // ========================================
    public async Task<NotificationDto> CreateNotificationAsync(
        Guid userId, 
        string title, 
        string message, 
        NotificationType type, 
        string? linkUrl = null, 
        string? icon = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            LinkUrl = linkUrl,
            Icon = icon ?? GetDefaultIcon(type),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return MapToDto(notification);
    }

    // ========================================
    // إنشاء وإرسال إشعار عبر SignalR (Realtime)
    // ========================================
    public async Task<NotificationDto> CreateAndSendNotificationAsync(
        Guid userId, 
        string title, 
        string message, 
        NotificationType type, 
        string? linkUrl = null, 
        string? icon = null)
    {
        var notification = await CreateNotificationAsync(userId, title, message, type, linkUrl, icon);

        // إرسال عبر SignalR
        await _hubContext.Clients.User(userId.ToString()).SendAsync(
            "ReceiveNotification", 
            notification);

        return notification;
    }

    // ========================================
    // الحصول على جميع الإشعارات
    // ========================================
    public async Task<NotificationListDto> GetNotificationsAsync(
        Guid userId, 
        int page = 1, 
        int pageSize = 20, 
        bool? isRead = null)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .AsQueryable();

        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead.Value);
        }

        var total = await query.CountAsync();
        var notifications = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new NotificationListDto
        {
            Notifications = notifications.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        };
    }

    // ========================================
    // وضع علامة كمقروء
    // ========================================
    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null || notification.IsRead)
            return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();

        return true;
    }

    // ========================================
    // وضع علامة على جميع الإشعارات كمقروءة
    // ========================================
    public async Task<int> MarkAllAsReadAsync(Guid userId)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            _context.Notifications.Update(notification);
        }

        await _context.SaveChangesAsync();

        // إرسال تحديث عبر SignalR
        await _hubContext.Clients.User(userId.ToString()).SendAsync(
            "NotificationsRead", 
            unreadNotifications.Count);

        return unreadNotifications.Count;
    }

    // ========================================
    // حذف إشعار
    // ========================================
    public async Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
            return false;

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        return true;
    }

    // ========================================
    // حذف جميع الإشعارات
    // ========================================
    public async Task<int> DeleteAllNotificationsAsync(Guid userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .ToListAsync();

        _context.Notifications.RemoveRange(notifications);
        await _context.SaveChangesAsync();

        return notifications.Count;
    }

    // ========================================
    // الحصول على عدد الإشعارات غير المقروءة
    // ========================================
    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    // ========================================
    // Helper Methods
    // ========================================

    private NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            LinkUrl = notification.LinkUrl,
            Icon = notification.Icon,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt
        };
    }

    private string GetDefaultIcon(NotificationType type)
    {
        return type switch
        {
            NotificationType.Course => "book-open",
            NotificationType.Payment => "credit-card",
            NotificationType.System => "bell",
            NotificationType.Message => "message-circle",
            NotificationType.TeacherRequest => "user-check",
            NotificationType.Enrollment => "users",
            NotificationType.Assignment => "clipboard-list",
            _ => "bell"
        };
    }
}