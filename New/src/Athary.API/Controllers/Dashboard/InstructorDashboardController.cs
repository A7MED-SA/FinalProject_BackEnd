using Athary.Application.Common;
using Athary.Application.DTOs.Dashboard;
using Athary.Application.Interfaces.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers.Dashboard;

[ApiController]
[Authorize(Roles = "Instructor")]
[Route("api/instructor/dashboard")]
public class InstructorDashboardController : ControllerBase
{
    private readonly IInstructorDashboardService _dashboardService;

    public InstructorDashboardController(IInstructorDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponse<InstructorOverviewDto>>> GetOverview(CancellationToken cancellationToken)
    {
        var instructorId = GetUserId();
        var result = await _dashboardService.GetInstructorOverviewAsync(instructorId, cancellationToken);
        return Ok(ApiResponse<InstructorOverviewDto>.SuccessResponse(result));
    }

    [HttpGet("courses")]
    public async Task<ActionResult<ApiResponse<List<ManagementCourseDto>>>> GetCourses(CancellationToken cancellationToken)
    {
        var instructorId = GetUserId();
        var result = await _dashboardService.GetInstructorCoursesAsync(instructorId, cancellationToken);
        return Ok(ApiResponse<List<ManagementCourseDto>>.SuccessResponse(result));
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<ApiResponse<InstructorDashboardRevenueDto>>> GetRevenue([FromQuery] int months = 12, CancellationToken cancellationToken = default)
    {
        var instructorId = GetUserId();
        var result = await _dashboardService.GetInstructorRevenueAsync(instructorId, months, cancellationToken);
        return Ok(ApiResponse<InstructorDashboardRevenueDto>.SuccessResponse(result));
    }

    [HttpGet("students")]
    public async Task<ActionResult<ApiResponse<InstructorDashboardStudentsDto>>> GetStudents(CancellationToken cancellationToken)
    {
        var instructorId = GetUserId();
        var result = await _dashboardService.GetInstructorStudentsAsync(instructorId, cancellationToken);
        return Ok(ApiResponse<InstructorDashboardStudentsDto>.SuccessResponse(result));
    }

    [HttpGet("pending-requests")]
    public async Task<ActionResult<ApiResponse<List<PendingEditRequestDto>>>> GetPendingRequests(CancellationToken cancellationToken)
    {
        var instructorId = GetUserId();
        var result = await _dashboardService.GetInstructorPendingRequestsAsync(instructorId, cancellationToken);
        return Ok(ApiResponse<List<PendingEditRequestDto>>.SuccessResponse(result));
    }

    [HttpGet("recent-reviews")]
    public async Task<ActionResult<ApiResponse<List<ReviewSummaryDto>>>> GetRecentReviews([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var instructorId = GetUserId();
        var result = await _dashboardService.GetInstructorRecentReviewsAsync(instructorId, limit, cancellationToken);
        return Ok(ApiResponse<List<ReviewSummaryDto>>.SuccessResponse(result));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (claim == null || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        return userId;
    }
}
