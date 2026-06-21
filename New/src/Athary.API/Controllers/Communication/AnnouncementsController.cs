using Athary.Application.Common;
using Athary.Application.DTOs.Communication;
using Athary.Application.Interfaces.Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers.Communication;

[ApiController]
[Route("api/announcements")]
[Authorize]
public sealed class AnnouncementsController : ControllerBase
{
    private readonly IAnnouncementService _announcementService;

    public AnnouncementsController(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AnnouncementResponse>>> Create([FromBody] CreateAnnouncementRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _announcementService.CreateAsync(userId, request, cancellationToken);
        return Ok(ApiResponse<AnnouncementResponse>.SuccessResponse(result));
    }

    [HttpPost("course/{courseId}")]
    public async Task<ActionResult<ApiResponse<AnnouncementResponse>>> CreateCourseAnnouncement(Guid courseId, [FromBody] CreateAnnouncementRequest request, CancellationToken cancellationToken)
    {
        request = request with { Target = "SpecificCourse", CourseId = courseId };
        var userId = GetUserId();
        var result = await _announcementService.CreateAsync(userId, request, cancellationToken);
        return Ok(ApiResponse<AnnouncementResponse>.SuccessResponse(result));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<AnnouncementListResponse>>> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _announcementService.GetFeedAsync(userId, page, pageSize, cancellationToken);
        return Ok(ApiResponse<AnnouncementListResponse>.SuccessResponse(result));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<AnnouncementResponse>>> Update(Guid id, [FromBody] UpdateAnnouncementRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _announcementService.UpdateAsync(id, userId, request, cancellationToken);
        return Ok(ApiResponse<AnnouncementResponse>.SuccessResponse(result));
    }

    [HttpPatch("{id}/deactivate")]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _announcementService.DeactivateAsync(id, userId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(new { }));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _announcementService.DeleteAsync(id, userId, cancellationToken);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (claim == null || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        return userId;
    }
}
