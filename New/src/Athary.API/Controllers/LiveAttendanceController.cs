using System.Security.Claims;
using Athary.Application.Common;
using Athary.Application.Interfaces.LiveSession;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/live-sessions/{sessionId:guid}/attendance")]
[Authorize]
public class LiveAttendanceController : ControllerBase
{
    private readonly ILiveAttendanceService _liveAttendanceService;

    public LiveAttendanceController(ILiveAttendanceService liveAttendanceService)
    {
        _liveAttendanceService = liveAttendanceService;
    }

    [HttpPost("join")]
    public async Task<ActionResult<ApiResponse<object>>> Join(Guid sessionId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _liveAttendanceService.JoinSessionAsync(sessionId, userId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Joined successfully"));
    }

    [HttpPost("leave")]
    public async Task<ActionResult<ApiResponse<object>>> Leave(Guid sessionId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _liveAttendanceService.LeaveSessionAsync(sessionId, userId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Left successfully"));
    }

    [HttpGet("count")]
    public async Task<ActionResult<ApiResponse<int>>> GetAttendanceCount(Guid sessionId, CancellationToken cancellationToken)
    {
        var count = await _liveAttendanceService.GetAttendanceCountAsync(sessionId, cancellationToken);
        return Ok(ApiResponse<int>.SuccessResponse(count));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(userIdClaim);
    }
}
