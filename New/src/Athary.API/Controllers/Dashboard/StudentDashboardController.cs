using Athary.Application.Common;
using Athary.Application.DTOs.Dashboard;
using Athary.Application.Interfaces.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers.Dashboard;

[ApiController]
[Authorize(Roles = "Student")]
[Route("api/student/dashboard")]
public class StudentDashboardController : ControllerBase
{
    private readonly IStudentDashboardService _dashboardService;

    public StudentDashboardController(IStudentDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponse<StudentOverviewDto>>> GetOverview(CancellationToken cancellationToken)
    {
        var studentId = GetUserId();
        var result = await _dashboardService.GetStudentOverviewAsync(studentId, cancellationToken);
        return Ok(ApiResponse<StudentOverviewDto>.SuccessResponse(result));
    }

    [HttpGet("courses")]
    public async Task<ActionResult<ApiResponse<List<StudentCourseDto>>>> GetCourses(CancellationToken cancellationToken)
    {
        var studentId = GetUserId();
        var result = await _dashboardService.GetStudentCoursesAsync(studentId, cancellationToken);
        return Ok(ApiResponse<List<StudentCourseDto>>.SuccessResponse(result));
    }

    [HttpGet("weekly-activity")]
    public async Task<ActionResult<ApiResponse<ChartSeriesDto>>> GetWeeklyActivity([FromQuery] int weeks = 4, CancellationToken cancellationToken = default)
    {
        var studentId = GetUserId();
        var result = await _dashboardService.GetStudentWeeklyActivityAsync(studentId, weeks, cancellationToken);
        return Ok(ApiResponse<ChartSeriesDto>.SuccessResponse(result));
    }

    [HttpGet("certificates")]
    public async Task<ActionResult<ApiResponse<List<StudentCertificateDto>>>> GetCertificates(CancellationToken cancellationToken)
    {
        var studentId = GetUserId();
        var result = await _dashboardService.GetStudentCertificatesAsync(studentId, cancellationToken);
        return Ok(ApiResponse<List<StudentCertificateDto>>.SuccessResponse(result));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (claim == null || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        return userId;
    }
}
