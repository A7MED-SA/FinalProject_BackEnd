using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/admin/courses")]
[Authorize(Roles = "Admin")]
public class AdminCourseController : ControllerBase
{
    private readonly ICourseService _courseService;

    public AdminCourseController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingCourses()
    {
        var result = await _courseService.GetPendingCoursesAsync();
        return Ok(backend_project.DTOs.ApiResponse<System.Collections.Generic.List<backend_project.DTOs.Course.CourseSummaryDto>>.SuccessResponse(result));
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveCourse(Guid id)
    {
        var adminId = GetUserId();
        await _courseService.ApproveCourseAsync(id, adminId);
        return Ok(backend_project.DTOs.ApiResponse<object>.SuccessResponse(null, "Course approved and published successfully."));
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectCourse(Guid id, [FromBody] RejectCourseRequestDto dto)
    {
        var adminId = GetUserId();
        await _courseService.RejectCourseAsync(id, adminId, dto.Reason);
        return Ok(backend_project.DTOs.ApiResponse<object>.SuccessResponse(null, "Course rejected."));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(userIdClaim);
    }
}

public class RejectCourseRequestDto
{
    public string Reason { get; set; } = string.Empty;
}
