using System.Security.Claims;
using Athary.Application.Common;
using Athary.Application.DTOs.Notification;
using Athary.Application.Interfaces.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/notifications/preferences")]
[Authorize]
[ApiExplorerSettings(GroupName = "User")]
[Tags("User - Notifications")]
public class NotificationPreferencesController : ControllerBase
{
    private readonly INotificationPreferenceService _service;

    public NotificationPreferencesController(INotificationPreferenceService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<NotificationPreferencesDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var preferences = await _service.GetByUserIdAsync(userId, cancellationToken);
        return Ok(ApiResponse<NotificationPreferencesDto>.SuccessResponse(preferences));
    }

    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<NotificationPreferencesDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdateNotificationPreferencesDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var preferences = await _service.UpdateAsync(userId, dto, cancellationToken);
        return Ok(ApiResponse<NotificationPreferencesDto>.SuccessResponse(preferences));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null ? Guid.Parse(claim.Value) : throw new UnauthorizedAccessException();
    }
}
