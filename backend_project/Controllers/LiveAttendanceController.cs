using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/live-sessions/{sessionId}/attendance")]
[Authorize]
public class LiveAttendanceController : ControllerBase
{
    private readonly ILiveAttendanceService _liveAttendanceService;

    public LiveAttendanceController(ILiveAttendanceService liveAttendanceService)
    {
        _liveAttendanceService = liveAttendanceService;
    }

    [HttpPost("join")]
    public async Task<IActionResult> Join(Guid sessionId)
    {
        try
        {
            var userId = GetUserId();
            await _liveAttendanceService.JoinSessionAsync(sessionId, userId);
            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Joined successfully" }));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("leave")]
    public async Task<IActionResult> Leave(Guid sessionId)
    {
        try
        {
            var userId = GetUserId();
            await _liveAttendanceService.LeaveSessionAsync(sessionId, userId);
            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Left successfully" }));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetAttendanceCount(Guid sessionId)
    {
        var count = await _liveAttendanceService.GetAttendanceCountAsync(sessionId);
        return Ok(ApiResponse<object>.SuccessResponse(new { count }));
    }

    private Guid GetUserId()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        return userId;
    }
}
