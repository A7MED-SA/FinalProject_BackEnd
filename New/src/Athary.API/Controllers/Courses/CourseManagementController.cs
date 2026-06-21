using Athary.Application.Common;
using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Athary.API.Controllers.Courses;

[ApiController]
[Route("api/management/courses")]
[Authorize]
public class CourseManagementController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly ILogger<CourseManagementController> _logger;

    public CourseManagementController(ICourseService courseService, ILogger<CourseManagementController> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CourseDetailsDto>>> CreateCourse(
        [FromBody] CreateCourseDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        _logger.LogInformation("Course {Title} is being created by instructor {UserId}", dto.Title, userId);
        var result = await _courseService.CreateCourseAsync(userId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, ApiResponse<CourseDetailsDto>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CourseDetailsDto>>> GetCourseById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _courseService.GetCourseByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<CourseDetailsDto>.SuccessResponse(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CourseDetailsDto>>> UpdateCourse(
        Guid id,
        [FromBody] UpdateCourseDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _courseService.UpdateCourseAsync(id, userId, dto, cancellationToken);
        return Ok(ApiResponse<CourseDetailsDto>.SuccessResponse(result));
    }

    [HttpPost("{id:guid}/requirements")]
    public async Task<ActionResult<ApiResponse<CourseRequirementDto>>> AddRequirement(
        Guid id,
        [FromBody] AddRequirementDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _courseService.AddRequirementAsync(id, userId, dto, cancellationToken);
        return Ok(ApiResponse<CourseRequirementDto>.SuccessResponse(result));
    }

    [HttpDelete("{id:guid}/requirements/{requirementId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveRequirement(
        Guid id,
        Guid requirementId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _courseService.RemoveRequirementAsync(id, requirementId, userId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Requirement removed successfully"));
    }

    [HttpPost("{id:guid}/outcomes")]
    public async Task<ActionResult<ApiResponse<CourseLearningOutcomeDto>>> AddLearningOutcome(
        Guid id,
        [FromBody] AddLearningOutcomeDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _courseService.AddLearningOutcomeAsync(id, userId, dto, cancellationToken);
        return Ok(ApiResponse<CourseLearningOutcomeDto>.SuccessResponse(result));
    }

    [HttpDelete("{id:guid}/outcomes/{outcomeId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveLearningOutcome(
        Guid id,
        Guid outcomeId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _courseService.RemoveLearningOutcomeAsync(id, outcomeId, userId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Outcome removed successfully"));
    }

    [HttpPost("{id:guid}/submit-for-review")]
    public async Task<ActionResult<ApiResponse<object>>> SubmitForReview(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _courseService.SubmitForReviewAsync(id, userId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Course submitted for review successfully"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCourse(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            await _courseService.DeleteCourseAsync(id, userId, cancellationToken);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Course deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("{id:guid}/schedule-deletion")]
    public async Task<ActionResult<ApiResponse<object>>> ScheduleDeletion(
        Guid id,
        [FromBody] ScheduleDeletionDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            await _courseService.ScheduleDeletionAsync(id, userId, dto.ScheduledDate, dto.Reason, cancellationToken);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Deletion scheduled successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("{id:guid}/cancel-scheduled-deletion")]
    public async Task<ActionResult<ApiResponse<object>>> CancelScheduledDeletion(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            await _courseService.CancelScheduledDeletionAsync(id, userId, cancellationToken);
            return Ok(ApiResponse<object>.SuccessResponse(null!, "Scheduled deletion cancelled successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("{id:guid}/deletion-status")]
    public async Task<ActionResult<ApiResponse<ScheduledDeletionStatusDto>>> GetDeletionStatus(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _courseService.GetScheduledDeletionStatusAsync(id, cancellationToken);
            return Ok(ApiResponse<ScheduledDeletionStatusDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{courseId:guid}/image")]
    public async Task<ActionResult<ApiResponse<CourseDetailsDto>>> SetCourseImage(
        Guid courseId,
        [FromBody] SetCourseImageRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            var result = await _courseService.SetCourseImageAsync(courseId, request.FileId, userId, cancellationToken);
            return Ok(ApiResponse<CourseDetailsDto>.SuccessResponse(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(userIdClaim);
    }
}
