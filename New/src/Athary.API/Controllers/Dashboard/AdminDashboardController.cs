using Athary.Application.Common;
using Athary.Application.DTOs.Dashboard;
using Athary.Application.Interfaces.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers.Dashboard;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/dashboard")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _dashboardService;

    public AdminDashboardController(IAdminDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponse<AdminOverviewDto>>> GetOverview(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetAdminOverviewAsync(cancellationToken);
        return Ok(ApiResponse<AdminOverviewDto>.SuccessResponse(result));
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<ApiResponse<AdminRevenueDto>>> GetRevenue([FromQuery] int months = 12, CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetAdminRevenueAsync(months, cancellationToken);
        return Ok(ApiResponse<AdminRevenueDto>.SuccessResponse(result));
    }

    [HttpGet("user-growth")]
    public async Task<ActionResult<ApiResponse<AdminUserGrowthDto>>> GetUserGrowth([FromQuery] int months = 6, CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetAdminUserGrowthAsync(months, cancellationToken);
        return Ok(ApiResponse<AdminUserGrowthDto>.SuccessResponse(result));
    }

    [HttpGet("enrollment-trend")]
    public async Task<ActionResult<ApiResponse<AdminEnrollmentTrendDto>>> GetEnrollmentTrend([FromQuery] int months = 12, CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetAdminEnrollmentTrendAsync(months, cancellationToken);
        return Ok(ApiResponse<AdminEnrollmentTrendDto>.SuccessResponse(result));
    }

    [HttpGet("top-courses")]
    public async Task<ActionResult<ApiResponse<List<TopCourseDto>>>> GetTopCourses([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetAdminTopCoursesAsync(limit, cancellationToken);
        return Ok(ApiResponse<List<TopCourseDto>>.SuccessResponse(result));
    }
}
