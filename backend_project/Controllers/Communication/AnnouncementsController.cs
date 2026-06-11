using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend_project.DTOs;
using backend_project.DTOs.Communication;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers.Communication;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnnouncementsController : ControllerBase
{
    private readonly IAnnouncementService _announcementService;

    public AnnouncementsController(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAnnouncementRequest request)
    {
        try
        {
            var result = await _announcementService.CreateAsync(GetUserId(), request);
            return CreatedAtAction(nameof(GetFeed), result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("course/{courseId}")]
    public async Task<IActionResult> CreateCourseAnnouncement(Guid courseId, [FromBody] CreateAnnouncementRequest request)
    {
        request.Target = "SpecificCourse";
        request.CourseId = courseId;
        try
        {
            var result = await _announcementService.CreateAsync(GetUserId(), request);
            return CreatedAtAction(nameof(GetFeed), result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _announcementService.GetFeedAsync(GetUserId(), page, pageSize);
        return Ok(ApiResponse<AnnouncementListResponse>.SuccessResponse(result));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAnnouncementRequest request)
    {
        try
        {
            var result = await _announcementService.UpdateAsync(id, GetUserId(), request);
            return Ok(ApiResponse<AnnouncementResponse>.SuccessResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        try
        {
            await _announcementService.DeactivateAsync(id, GetUserId());
            return Ok(ApiResponse<object>.SuccessResponse(new { }));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _announcementService.DeleteAsync(id, GetUserId());
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }
}
