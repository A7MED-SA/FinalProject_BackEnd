using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.DTOs.ContentProgress;
using backend_project.DTOs.Enrollment;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly IContentProgressService _contentProgressService;

    public EnrollmentsController(IEnrollmentService enrollmentService, IContentProgressService contentProgressService)
    {
        _enrollmentService = enrollmentService;
        _contentProgressService = contentProgressService;
    }

    [HttpPost]
    public async Task<IActionResult> Enroll([FromBody] CreateEnrollmentDto createDto)
    {
        // Force the user ID to the currently logged in user to prevent enrolling other users
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        createDto.UserId = userId;

        try
        {
            var result = await _enrollmentService.EnrollUserAsync(createDto);
            return CreatedAtAction(nameof(GetEnrollment), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyEnrollments()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var enrollments = await _enrollmentService.GetUserEnrollmentsAsync(userId);
        return Ok(enrollments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEnrollment(Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var enrollment = await _enrollmentService.GetEnrollmentDetailsAsync(id, userId);
            return Ok(enrollment);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{enrollmentId}/progress")]
    public async Task<IActionResult> GetProgress(Guid enrollmentId)
    {
        var progress = await _contentProgressService.GetProgressForEnrollmentAsync(enrollmentId);
        return Ok(ApiResponse<IEnumerable<ContentProgressDto>>.SuccessResponse(progress));
    }

    [HttpPut("{enrollmentId}/progress")]
    public async Task<IActionResult> UpdateProgress(Guid enrollmentId, [FromBody] UpdateProgressDto updateDto)
    {
        try
        {
            var result = await _contentProgressService.UpdateProgressAsync(enrollmentId, updateDto);
            return Ok(ApiResponse<ContentProgressDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ContentProgressDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("{enrollmentId}/progress/{contentType}/{contentId}/complete")]
    public async Task<IActionResult> MarkCompleted(Guid enrollmentId, string contentType, Guid contentId)
    {
        try
        {
            if (!Enum.TryParse<Models.ContentType>(contentType, true, out var parsedType))
                return BadRequest(ApiResponse<object>.FailureResponse("Invalid content type."));

            var result = await _contentProgressService.MarkCompletedAsync(enrollmentId, contentId, parsedType);
            return Ok(ApiResponse<ContentProgressDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ContentProgressDto>.FailureResponse(ex.Message));
        }
    }
}
