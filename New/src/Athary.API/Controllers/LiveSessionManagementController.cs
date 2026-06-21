using Athary.Application.Common;
using Athary.Application.DTOs.LiveSession;
using Athary.Application.Interfaces.LiveSession;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/courses/{courseId:guid}/live-sessions")]
[Authorize(Roles = "Instructor")]
public class LiveSessionManagementController : ControllerBase
{
    private readonly ILiveSessionService _liveSessionService;

    public LiveSessionManagementController(ILiveSessionService liveSessionService)
    {
        _liveSessionService = liveSessionService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<LiveSessionResponseDto>>>> GetSessions(Guid courseId, CancellationToken cancellationToken)
    {
        var result = await _liveSessionService.GetSessionsForCourseAsync(courseId, cancellationToken);
        return Ok(ApiResponse<List<LiveSessionResponseDto>>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LiveSessionResponseDto>>> CreateSession(Guid courseId, [FromBody] CreateLiveSessionDto createDto, CancellationToken cancellationToken)
    {
        var result = await _liveSessionService.CreateSessionAsync(createDto, cancellationToken);
        return Ok(ApiResponse<LiveSessionResponseDto>.SuccessResponse(result));
    }

    [HttpPut("{sessionId:guid}/status")]
    public async Task<ActionResult<ApiResponse<LiveSessionResponseDto>>> UpdateStatus(Guid courseId, Guid sessionId, [FromBody] UpdateLiveSessionStatusDto statusDto, CancellationToken cancellationToken)
    {
        var result = await _liveSessionService.UpdateStatusAsync(sessionId, statusDto, cancellationToken);
        return Ok(ApiResponse<LiveSessionResponseDto>.SuccessResponse(result));
    }

    [HttpDelete("{sessionId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSession(Guid courseId, Guid sessionId, CancellationToken cancellationToken)
    {
        var result = await _liveSessionService.DeleteSessionAsync(sessionId, cancellationToken);
        if (!result)
            return NotFound(ApiResponse<object>.FailureResponse("Live session not found."));

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Deleted"));
    }
}
