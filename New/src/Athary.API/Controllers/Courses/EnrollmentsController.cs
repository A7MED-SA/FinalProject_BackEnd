using Athary.Application.Common;
using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Athary.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Athary.API.Controllers.Courses;

[ApiController]
[Route("api/enrollments")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly IContentProgressService _contentProgressService;

    public EnrollmentsController(
        IEnrollmentService enrollmentService,
        IContentProgressService contentProgressService)
    {
        _enrollmentService = enrollmentService;
        _contentProgressService = contentProgressService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<EnrollmentResponseDto>>> Enroll(
        [FromBody] CreateEnrollmentDto createDto,
        CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        createDto = createDto with { UserId = userId };

        try
        {
            var result = await _enrollmentService.EnrollUserAsync(createDto, cancellationToken);
            return CreatedAtAction(nameof(GetEnrollment), new { id = result.Id }, ApiResponse<EnrollmentResponseDto>.SuccessResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<EnrollmentResponseDto>>>> GetMyEnrollments(CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var enrollments = await _enrollmentService.GetUserEnrollmentsAsync(userId, cancellationToken);
        return Ok(ApiResponse<IEnumerable<EnrollmentResponseDto>>.SuccessResponse(enrollments));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<EnrollmentDetailDto>>> GetEnrollment(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        try
        {
            var enrollment = await _enrollmentService.GetEnrollmentDetailsAsync(id, userId, cancellationToken);
            return Ok(ApiResponse<EnrollmentDetailDto>.SuccessResponse(enrollment));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("{enrollmentId:guid}/progress")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ContentProgressDto>>>> GetProgress(
        Guid enrollmentId,
        CancellationToken cancellationToken)
    {
        var progress = await _contentProgressService.GetProgressForEnrollmentAsync(enrollmentId, cancellationToken);
        return Ok(ApiResponse<IEnumerable<ContentProgressDto>>.SuccessResponse(progress.ToList()));
    }

    [HttpPut("{enrollmentId:guid}/progress")]
    public async Task<ActionResult<ApiResponse<ContentProgressDto>>> UpdateProgress(
        Guid enrollmentId,
        [FromBody] UpdateProgressDto updateDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _contentProgressService.UpdateProgressAsync(enrollmentId, updateDto, cancellationToken);
            return Ok(ApiResponse<ContentProgressDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ContentProgressDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("{enrollmentId:guid}/progress/{contentType}/{contentId:guid}/complete")]
    public async Task<ActionResult<ApiResponse<ContentProgressDto>>> MarkCompleted(
        Guid enrollmentId,
        string contentType,
        Guid contentId,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ContentType>(contentType, true, out var parsedType))
            return BadRequest(ApiResponse<object>.FailureResponse("Invalid content type."));

        try
        {
            var result = await _contentProgressService.MarkCompletedAsync(enrollmentId, contentId, parsedType, cancellationToken);
            return Ok(ApiResponse<ContentProgressDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ContentProgressDto>.FailureResponse(ex.Message));
        }
    }
}
