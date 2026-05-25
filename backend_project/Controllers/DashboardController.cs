using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.DTOs.Dashboard;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IStudentDashboardService _studentService;
    private readonly IInstructorDashboardService _instructorService;
    private readonly IActivityLogService _activityLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DashboardController(
        IStudentDashboardService studentService,
        IInstructorDashboardService instructorService,
        IActivityLogService activityLogService,
        IHttpContextAccessor httpContextAccessor)
    {
        _studentService = studentService;
        _instructorService = instructorService;
        _activityLogService = activityLogService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet("student")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetStudentDashboard()
    {
        var userId = GetUserId();
        var result = await _studentService.GetDashboardAsync(userId);
        return Ok(ApiResponse<StudentDashboardDto>.SuccessResponse(result));
    }

    [HttpGet("instructor")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> GetInstructorDashboard()
    {
        var userId = GetUserId();
        var result = await _instructorService.GetDashboardAsync(userId);

        await _activityLogService.LogActivityAsync(
            userId,
            "DashboardViewed_Revenue",
            "Instructor viewed revenue dashboard",
            GetIpAddress());

        return Ok(ApiResponse<InstructorDashboardDto>.SuccessResponse(result));
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
