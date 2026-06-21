using Athary.Application.Common;
using Athary.Application.DTOs.Dashboard;
using Athary.Application.Interfaces.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/management")]
[Authorize(Roles = "Instructor")]
public sealed class ManagementCoursesController : ControllerBase
{
    private readonly IInstructorDashboardService _instructorService;

    public ManagementCoursesController(IInstructorDashboardService instructorService)
    {
        _instructorService = instructorService;
    }

    [HttpGet("courses")]
    public async Task<ActionResult<ApiResponse<List<ManagementCourseDto>>>> GetCourses(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _instructorService.GetInstructorCoursesAsync(userId, cancellationToken);
        return Ok(ApiResponse<List<ManagementCourseDto>>.SuccessResponse(result));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (claim == null || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        return userId;
    }
}
