using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend_project.DTOs.Course;

using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/management/courses")]
[Authorize] // Instructors
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
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto)
    {
            _logger.LogInformation("Course {Title} {Slug} is creating ...", dto.Title, dto.Slug);
        var userId = GetUserId();
        var result = await _courseService.CreateCourseAsync(userId, dto);
        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, backend_project.DTOs.ApiResponse<CourseDetailsDto>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCourseById(Guid id)
    {
        var result = await _courseService.GetCourseByIdAsync(id);
        return Ok(backend_project.DTOs.ApiResponse<CourseDetailsDto>.SuccessResponse(result));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] UpdateCourseDto dto)
    {
        var userId = GetUserId();
        var result = await _courseService.UpdateCourseAsync(id, userId, dto);
        return Ok(backend_project.DTOs.ApiResponse<CourseDetailsDto>.SuccessResponse(result));
    }

    [HttpPost("{id}/requirements")]
    public async Task<IActionResult> AddRequirement(Guid id, [FromBody] AddRequirementDto dto)
    {
        var userId = GetUserId();
        var result = await _courseService.AddRequirementAsync(id, userId, dto);
        return Ok(backend_project.DTOs.ApiResponse<CourseRequirementDto>.SuccessResponse(result));
    }

    [HttpDelete("{id}/requirements/{requirementId}")]
    public async Task<IActionResult> RemoveRequirement(Guid id, Guid requirementId)
    {
        var userId = GetUserId();
        await _courseService.RemoveRequirementAsync(id, requirementId, userId);
        return Ok(backend_project.DTOs.ApiResponse<object>.SuccessResponse(null, "Requirement removed successfully"));
    }

    [HttpPost("{id}/outcomes")]
    public async Task<IActionResult> AddLearningOutcome(Guid id, [FromBody] AddLearningOutcomeDto dto)
    {
        var userId = GetUserId();
        var result = await _courseService.AddLearningOutcomeAsync(id, userId, dto);
        return Ok(backend_project.DTOs.ApiResponse<CourseLearningOutcomeDto>.SuccessResponse(result));
    }

    [HttpDelete("{id}/outcomes/{outcomeId}")]
    public async Task<IActionResult> RemoveLearningOutcome(Guid id, Guid outcomeId)
    {
        var userId = GetUserId();
        await _courseService.RemoveLearningOutcomeAsync(id, outcomeId, userId);
        return Ok(backend_project.DTOs.ApiResponse<object>.SuccessResponse(null, "Outcome removed successfully"));
    }

    [HttpPost("{id}/submit-for-review")]
    public async Task<IActionResult> SubmitForReview(Guid id)
    {
        var userId = GetUserId();
        await _courseService.SubmitForReviewAsync(id, userId);
        return Ok(backend_project.DTOs.ApiResponse<object>.SuccessResponse(null, "Course submitted for review successfully"));
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourse(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _courseService.DeleteCourseAsync(id, userId);
            return Ok(backend_project.DTOs.ApiResponse<object>.SuccessResponse(null, "Course deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(backend_project.DTOs.ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, backend_project.DTOs.ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{courseId}/image")]
[Authorize]
public async Task<IActionResult> SetCourseImage(
    Guid courseId, 
    [FromBody] SetCourseImageRequest request
    ) // ⚠️ طريقة استخراج الـ UserID حسب الـ Auth عندك
{
    try
    {
    var userId = GetUserId();
        var result = await _courseService.SetCourseImageAsync(
            courseId, request.FileId, userId);
        return Ok(result);
    }
    catch (UnauthorizedAccessException ex)
    {
        return StatusCode(403, new { message = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(userIdClaim);
    }
}
