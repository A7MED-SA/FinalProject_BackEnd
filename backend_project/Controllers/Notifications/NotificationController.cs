using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend_project.DTOs.Notifications;
using backend_project.Services.Notifications;

namespace backend_project.Controllers.Notifications;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// الحصول على جميع الإشعارات
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isRead = null)
    {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _notificationService.GetNotificationsAsync(
            Guid.Parse(userId), 
            page, 
            pageSize, 
            isRead);

        return Ok(result);
    }

    /// <summary>
    /// الحصول على عدد الإشعارات غير المقروءة
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var count = await _notificationService.GetUnreadCountAsync(Guid.Parse(userId));
        
        return Ok(new { count });
    }

    /// <summary>
    /// وضع علامة على إشعار كمقروء
    /// </summary>
    [HttpPatch("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId)
    {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await _notificationService.MarkAsReadAsync(
            notificationId, 
            Guid.Parse(userId));

        if (!success)
            return NotFound(new { message = "الإشعار غير موجود" });

        return Ok(new { message = "تم وضع علامة كمقروء" });
    }

    /// <summary>
    /// وضع علامة على جميع الإشعارات كمقروءة
    /// </summary>
    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var count = await _notificationService.MarkAllAsReadAsync(Guid.Parse(userId));

        return Ok(new { message = $"تم وضع علامة على {count} إشعارات كمقروءة" });
    }

    /// <summary>
    /// حذف إشعار
    /// </summary>
    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> DeleteNotification(Guid notificationId)
    {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await _notificationService.DeleteNotificationAsync(
            notificationId, 
            Guid.Parse(userId));

        if (!success)
            return NotFound(new { message = "الإشعار غير موجود" });

        return Ok(new { message = "تم حذف الإشعار" });
    }

    /// <summary>
    /// حذف جميع الإشعارات
    /// </summary>
    [HttpDelete("clear-all")]
    public async Task<IActionResult> ClearAllNotifications()
    {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var count = await _notificationService.DeleteAllNotificationsAsync(Guid.Parse(userId));

        return Ok(new { message = $"تم حذف {count} إشعارات" });
    }
}