using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend_project.DTOs.EditRequest;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/admin/courses")]
[Authorize(Roles = "Admin")]
public class AdminCourseController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly ICourseEditApprovalService _editApprovalService;

    public AdminCourseController(
        ICourseService courseService,
        ICourseEditApprovalService editApprovalService)
    {
        _courseService = courseService;
        _editApprovalService = editApprovalService;
    }

    // === Existing Course Approval ===

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

    // === Edit Request Management ===

    /// <summary>
    /// Lists all edit requests (default: pending) with filtering and pagination
    /// </summary>
    [HttpGet("edit-requests")]
    public async Task<IActionResult> GetEditRequests([FromQuery] EditRequestFilterDto filter)
    {
        var result = await _editApprovalService.GetPendingRequestsAsync(filter);
        return Ok(backend_project.DTOs.ApiResponse<PagedResult<EditRequestSummaryDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Gets the full details of a specific edit request (with diffs and impact analysis)
    /// </summary>
    [HttpGet("edit-requests/{requestId}")]
    public async Task<IActionResult> GetEditRequestDetails(Guid requestId)
    {
        var result = await _editApprovalService.GetRequestDetailsAsync(requestId);
        return Ok(backend_project.DTOs.ApiResponse<EditRequestDetailDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Admin approves or rejects an edit request
    /// </summary>
    [HttpPost("edit-requests/{requestId}/review")]
    public async Task<IActionResult> ReviewEditRequest(Guid requestId, [FromBody] ReviewRequestDto dto)
    {
        var adminId = GetUserId();
        var result = await _editApprovalService.ReviewRequestAsync(
            requestId, adminId, dto.Approve, dto.Notes);
        return Ok(backend_project.DTOs.ApiResponse<EditResultDto>.SuccessResponse(result));
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
