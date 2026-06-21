using Athary.Application.Common;
using Athary.Application.DTOs.Review;
using Athary.Application.Interfaces.Review;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

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
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> CreateReview([FromBody] CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _reviewService.CreateReviewAsync(userId, request, cancellationToken);
        return Ok(ApiResponse<ReviewResponse>.SuccessResponse(result));
    }

    [HttpGet("course/{courseId}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<ReviewResponse>>>> GetCourseReviews(Guid courseId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _reviewService.GetCourseReviewsAsync(courseId, page, pageSize, cancellationToken);
        return Ok(ApiResponse<List<ReviewResponse>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ReviewDetailResponse>>> GetReview(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reviewService.GetReviewByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ReviewDetailResponse>.SuccessResponse(result));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> UpdateReview(Guid id, [FromBody] CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _reviewService.UpdateReviewAsync(id, userId, request, cancellationToken);
        return Ok(ApiResponse<ReviewResponse>.SuccessResponse(result));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _reviewService.DeleteReviewAsync(id, userId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/helpful")]
    [Authorize]
    public async Task<IActionResult> ToggleHelpful(Guid id, [FromBody] ReviewHelpfulRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _reviewService.ToggleHelpfulAsync(id, userId, request, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(new { message = "Helpful status updated." }));
    }

    [HttpPost("{id}/flag")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> FlagReview(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _reviewService.FlagReviewAsync(id, userId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(new { message = "Review flagged for moderation." }));
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<ReviewDetailResponse>>>> GetPendingReviews(CancellationToken cancellationToken)
    {
        var result = await _reviewService.GetPendingReviewsAsync(cancellationToken);
        return Ok(ApiResponse<List<ReviewDetailResponse>>.SuccessResponse(result));
    }

    [HttpPut("{id}/moderate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> ModerateReview(Guid id, [FromBody] ModerateReviewRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _reviewService.ModerateReviewAsync(id, userId, request, cancellationToken);
        return Ok(ApiResponse<ReviewResponse>.SuccessResponse(result));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (claim == null || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        return userId;
    }
}
