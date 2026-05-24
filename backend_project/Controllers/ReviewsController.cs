using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.DTOs.Review;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var result = await _reviewService.CreateReviewAsync(userId.Value, request);
            return CreatedAtAction(nameof(GetReview), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("course/{courseId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCourseReviews(Guid courseId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var reviews = await _reviewService.GetCourseReviewsAsync(courseId, page, pageSize);
        return Ok(ApiResponse<IEnumerable<ReviewResponse>>.SuccessResponse(reviews));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReview(Guid id)
    {
        try
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            return Ok(ApiResponse<ReviewDetailResponse>.SuccessResponse(review));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateReview(Guid id, [FromBody] CreateReviewRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var result = await _reviewService.UpdateReviewAsync(id, userId.Value, request);
            return Ok(ApiResponse<ReviewResponse>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            await _reviewService.DeleteReviewAsync(id, userId.Value);
            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Review deleted successfully." }));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("{id}/helpful")]
    [Authorize]
    public async Task<IActionResult> ToggleHelpful(Guid id, [FromBody] ReviewHelpfulRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var result = await _reviewService.ToggleHelpfulAsync(id, userId.Value, request);
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("{id}/flag")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> FlagReview(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            await _reviewService.FlagReviewAsync(id, userId.Value);
            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Review flagged for moderation." }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingReviews()
    {
        var reviews = await _reviewService.GetPendingReviewsAsync();
        return Ok(ApiResponse<IEnumerable<ReviewDetailResponse>>.SuccessResponse(reviews));
    }

    [HttpPut("{id}/moderate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ModerateReview(Guid id, [FromBody] ModerateReviewRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var result = await _reviewService.ModerateReviewAsync(id, userId.Value, request);
            return Ok(ApiResponse<ReviewResponse>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(value) || !Guid.TryParse(value, out var userId))
            return null;
        return userId;
    }
}
