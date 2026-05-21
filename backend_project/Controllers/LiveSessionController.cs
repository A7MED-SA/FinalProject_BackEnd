using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.DTOs.LiveSession;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/courses/{courseId}/live-sessions")]
[Authorize(Roles = "Instructor")]
public class LiveSessionController : ControllerBase
{
    private readonly ILiveSessionService _liveSessionService;

    public LiveSessionController(ILiveSessionService liveSessionService)
    {
        _liveSessionService = liveSessionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSessions(Guid courseId)
    {
        var result = await _liveSessionService.GetSessionsForCourseAsync(courseId);
        return Ok(ApiResponse<IEnumerable<LiveSessionResponseDto>>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<IActionResult> CreateSession(Guid courseId, [FromBody] CreateLiveSessionDto createDto)
    {
        try
        {
            var result = await _liveSessionService.CreateSessionAsync(createDto);
            return Ok(ApiResponse<LiveSessionResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LiveSessionResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{sessionId}/status")]
    public async Task<IActionResult> UpdateStatus(Guid courseId, Guid sessionId, [FromBody] UpdateLiveSessionStatusDto statusDto)
    {
        try
        {
            var result = await _liveSessionService.UpdateStatusAsync(sessionId, statusDto);
            return Ok(ApiResponse<LiveSessionResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LiveSessionResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{sessionId}")]
    public async Task<IActionResult> DeleteSession(Guid courseId, Guid sessionId)
    {
        try
        {
            var result = await _liveSessionService.DeleteSessionAsync(sessionId);
            if (!result)
                return NotFound(ApiResponse<object>.FailureResponse("Live session not found."));

            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Deleted" }));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    private Guid GetUserId()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        return userId;
    }
}
