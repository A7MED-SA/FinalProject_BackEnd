using Athary.Application.Common;
using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Athary.API.Controllers.Courses;

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

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ApiResponse<object>>> ApproveCourse(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var adminId = GetUserId();
            await _courseService.ApproveCourseAsync(id, adminId, cancellationToken);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Course approved successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ApiResponse<object>>> RejectCourse(
        Guid id,
        [FromBody] RejectCourseRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var adminId = GetUserId();
            await _courseService.RejectCourseAsync(id, adminId, request.Reason, cancellationToken);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Course rejected successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("edit-requests")]
    public async Task<ActionResult<ApiResponse<PagedList<EditRequestSummaryDto>>>> GetEditRequests(
        [FromQuery] EditRequestFilterDto filter,
        CancellationToken cancellationToken)
    {
        var result = await _editApprovalService.GetPendingRequestsAsync(filter, cancellationToken);
        return Ok(ApiResponse<PagedList<EditRequestSummaryDto>>.SuccessResponse(result));
    }

    [HttpGet("edit-requests/{requestId:guid}")]
    public async Task<ActionResult<ApiResponse<EditRequestDetailDto>>> GetEditRequestDetails(
        Guid requestId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _editApprovalService.GetRequestDetailsAsync(requestId, cancellationToken);
            return Ok(ApiResponse<EditRequestDetailDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<EditRequestDetailDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("edit-requests/{requestId:guid}/review")]
    public async Task<ActionResult<ApiResponse<EditResultDto>>> ReviewEditRequest(
        Guid requestId,
        [FromBody] ReviewRequestDto dto,
        CancellationToken cancellationToken)
    {
        var adminId = GetUserId();
        try
        {
            var result = await _editApprovalService.ReviewRequestAsync(requestId, adminId, dto.Approve, dto.Notes, cancellationToken);
            return Ok(ApiResponse<EditResultDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<EditResultDto>.FailureResponse(ex.Message));
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(userIdClaim);
    }
}

public sealed record RejectCourseRequest
{
    public string Reason { get; init; } = string.Empty;
}
