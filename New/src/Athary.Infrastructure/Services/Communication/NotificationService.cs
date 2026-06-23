using Athary.Application.DTOs.Notification;
using Athary.Application.Interfaces.Notification;
using Athary.Domain.Enums;
using Athary.Domain.Interfaces;
using Athary.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using DomainNotification = Athary.Domain.Entities.Notification;

namespace Athary.Infrastructure.Services.Communication;

public sealed class NotificationService : INotificationService
{
    private readonly IRepository<DomainNotification> _notificationRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(
        IRepository<DomainNotification> notificationRepo,
        IUnitOfWork unitOfWork,
        IHubContext<NotificationHub> hubContext)
    {
        _notificationRepo = notificationRepo;
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
    }

    public async Task<NotificationDto> CreateNotificationAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        string? linkUrl = null,
        string? icon = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new DomainNotification
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

        await _notificationRepo.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(notification);
    }

    public async Task<NotificationDto> CreateAndSendNotificationAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        string? linkUrl = null,
        string? icon = null,
        CancellationToken cancellationToken = default)
    {
        var notification = await CreateNotificationAsync(userId, title, message, type, linkUrl, icon, cancellationToken);

        await _hubContext.Clients.User(userId.ToString()).SendAsync(
            "ReceiveNotification",
            notification,
            cancellationToken);

        return notification;
    }

    public async Task<NotificationListDto> GetNotificationsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        bool? isRead = null,
        CancellationToken cancellationToken = default)
    {
        var predicate = isRead.HasValue
            ? (System.Linq.Expressions.Expression<Func<DomainNotification, bool>>)(n => n.UserId == userId && n.IsRead == isRead.Value)
            : (System.Linq.Expressions.Expression<Func<DomainNotification, bool>>)(n => n.UserId == userId);

        var notifications = await _notificationRepo.FindAsync(predicate, cancellationToken);

        var ordered = notifications.OrderByDescending(n => n.CreatedAt).ToList();
        var total = ordered.Count;

        var paged = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new NotificationListDto
        {
            Notifications = paged.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        };
    }

    public async Task<bool> MarkAsReadAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepo.FirstOrDefaultAsync(
            n => n.Id == notificationId && n.UserId == userId,
            cancellationToken: cancellationToken);

        if (notification == null || notification.IsRead)
            return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        await _notificationRepo.UpdateAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<int> MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var unreadNotifications = await _notificationRepo.FindAsync(
            n => n.UserId == userId && !n.IsRead,
            cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _notificationRepo.UpdateAsync(notification, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _hubContext.Clients.User(userId.ToString()).SendAsync(
            "NotificationsRead",
            unreadNotifications.Count,
            cancellationToken);

        return unreadNotifications.Count;
    }

    public async Task<bool> DeleteNotificationAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepo.FirstOrDefaultAsync(
            n => n.Id == notificationId && n.UserId == userId,
            cancellationToken: cancellationToken);

        if (notification == null)
            return false;

        await _notificationRepo.DeleteAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<int> DeleteAllNotificationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepo.FindAsync(
            n => n.UserId == userId,
            cancellationToken);

        foreach (var notification in notifications)
        {
            await _notificationRepo.DeleteAsync(notification, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return notifications.Count;
    }

    public async Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _notificationRepo.CountAsync(
            n => n.UserId == userId && !n.IsRead,
            cancellationToken);
    }

    private static NotificationDto MapToDto(DomainNotification notification)
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

    private static string GetDefaultIcon(NotificationType type)
    {
        return type switch
        {
            NotificationType.Course => "book-open",
            NotificationType.Payment => "credit-card",
            NotificationType.System => "bell",
            NotificationType.Message => "message-circle",
            NotificationType.InstructorRequest => "user-check",
            NotificationType.Enrollment => "users",
            NotificationType.Assignment => "clipboard-list",
            _ => "bell"
        };
    }
}
