using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.DTOs.Dashboard;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _adminService;
    private readonly IActivityLogService _activityLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdminDashboardController(
        IAdminDashboardService adminService,
        IActivityLogService activityLogService,
        IHttpContextAccessor httpContextAccessor)
    {
        _adminService = adminService;
        _activityLogService = activityLogService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        var userId = GetUserId();
        var result = await _adminService.GetOverviewAsync();

        await _activityLogService.LogActivityAsync(
            userId,
            "DashboardViewed_AdminOverview",
            "Admin viewed platform overview dashboard",
            GetIpAddress());

        return Ok(ApiResponse<AdminOverviewDto>.SuccessResponse(result));
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetMonthlyRevenue([FromQuery] int months = 12)
    {
        var userId = GetUserId();
        var result = await _adminService.GetMonthlyRevenueAsync(months);

        await _activityLogService.LogActivityAsync(
            userId,
            "DashboardViewed_Revenue",
            $"Admin viewed monthly revenue breakdown ({months} months)",
            GetIpAddress());

        return Ok(ApiResponse<List<MonthlyRevenueDto>>.SuccessResponse(result));
    }

    [HttpGet("user-growth")]
    public async Task<IActionResult> GetUserGrowth([FromQuery] int months = 6)
    {
        var result = await _adminService.GetUserGrowthAsync(months);
        return Ok(ApiResponse<List<UserGrowthDto>>.SuccessResponse(result));
    }

    [HttpGet("enrollment-trends")]
    public async Task<IActionResult> GetEnrollmentTrends([FromQuery] int months = 12)
    {
        var result = await _adminService.GetEnrollmentTrendsAsync(months);
        return Ok(ApiResponse<List<EnrollmentTrendDto>>.SuccessResponse(result));
    }

    private Guid GetUserId()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        return userId;
    }

    private string GetIpAddress()
    {
        return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
