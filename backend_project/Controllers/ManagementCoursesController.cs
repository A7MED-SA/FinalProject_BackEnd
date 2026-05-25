using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.DTOs.Dashboard;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/management")]
[Authorize(Roles = "Instructor")]
public class ManagementCoursesController : ControllerBase
{
    private readonly IInstructorDashboardService _instructorService;

    public ManagementCoursesController(IInstructorDashboardService instructorService)
    {
        _instructorService = instructorService;
    }

    [HttpGet("courses")]
    public async Task<IActionResult> GetCourses([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await _instructorService.GetCoursesAsync(userId, page, pageSize);
        return Ok(ApiResponse<PaginatedResult<ManagementCourseDto>>.SuccessResponse(result));
    }

    private Guid GetUserId()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        return userId;
    }
}
