using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs.Enrollment;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
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
}
