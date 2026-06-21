using System.Security.Claims;
using Athary.Application.Common;
using Athary.Application.DTOs.Notification;
using Athary.Application.Interfaces.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<NotificationListDto>>> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isRead = null,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var result = await _notificationService.GetNotificationsAsync(Guid.Parse(userId), page, pageSize, isRead, cancellationToken);
        return Ok(ApiResponse<NotificationListDto>.SuccessResponse(result));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var count = await _notificationService.GetUnreadCountAsync(Guid.Parse(userId), cancellationToken);
        return Ok(ApiResponse<int>.SuccessResponse(count));
    }

    [HttpPatch("{notificationId:guid}/read")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAsRead(Guid notificationId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var success = await _notificationService.MarkAsReadAsync(notificationId, Guid.Parse(userId), cancellationToken);
        if (!success) return NotFound(ApiResponse<object>.FailureResponse("Notification not found"));

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Marked as read"));
    }

    [HttpPost("mark-all-read")]
    public async Task<ActionResult<ApiResponse<int>>> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var count = await _notificationService.MarkAllAsReadAsync(Guid.Parse(userId), cancellationToken);
        return Ok(ApiResponse<int>.SuccessResponse(count, $"{count} notifications marked as read"));
    }

    [HttpDelete("{notificationId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteNotification(Guid notificationId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var success = await _notificationService.DeleteNotificationAsync(notificationId, Guid.Parse(userId), cancellationToken);
        if (!success) return NotFound(ApiResponse<object>.FailureResponse("Notification not found"));

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Notification deleted"));
    }

    [HttpDelete("clear-all")]
    public async Task<ActionResult<ApiResponse<int>>> ClearAllNotifications(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var count = await _notificationService.DeleteAllNotificationsAsync(Guid.Parse(userId), cancellationToken);
        return Ok(ApiResponse<int>.SuccessResponse(count, $"{count} notifications deleted"));
    }
}
