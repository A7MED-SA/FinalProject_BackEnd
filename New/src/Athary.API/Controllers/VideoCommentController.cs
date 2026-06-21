using Athary.Application.Common;
using Athary.Application.DTOs.VideoComment;
using Athary.Application.Interfaces.VideoComment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/videos/{videoId}/comments")]
[Authorize]
public class VideoCommentController : ControllerBase
{
    private readonly IVideoCommentService _videoCommentService;

    public VideoCommentController(IVideoCommentService videoCommentService)
    {
        _videoCommentService = videoCommentService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<CommentResponseDto>>>> GetComments(Guid videoId, CancellationToken cancellationToken)
    {
        Guid? userId = null;
        if (User.Identity?.IsAuthenticated == true)
            userId = GetUserId();

        var result = await _videoCommentService.GetCommentsForVideoAsync(videoId, userId, cancellationToken);
        return Ok(ApiResponse<List<CommentResponseDto>>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CommentResponseDto>>> AddComment(Guid videoId, [FromBody] CreateCommentDto createDto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _videoCommentService.AddCommentAsync(videoId, userId, createDto, cancellationToken);
        return CreatedAtAction(nameof(GetComments), new { videoId }, ApiResponse<CommentResponseDto>.SuccessResponse(result));
    }

    [HttpPut("{commentId}")]
    public async Task<ActionResult<ApiResponse<CommentResponseDto>>> UpdateComment(Guid videoId, Guid commentId, [FromBody] CreateCommentDto updateDto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _videoCommentService.UpdateCommentAsync(commentId, userId, updateDto, cancellationToken);
        return Ok(ApiResponse<CommentResponseDto>.SuccessResponse(result));
    }

    [HttpDelete("{commentId}")]
    public async Task<IActionResult> DeleteComment(Guid videoId, Guid commentId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var deleted = await _videoCommentService.DeleteCommentAsync(commentId, userId, cancellationToken);
        if (!deleted)
            return NotFound(ApiResponse<object>.FailureResponse("Comment not found."));
        return NoContent();
    }

    [HttpPost("{commentId}/like")]
    public async Task<ActionResult<ApiResponse<CommentLikeResponseDto>>> ToggleLike(Guid videoId, Guid commentId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _videoCommentService.ToggleLikeAsync(commentId, userId, cancellationToken);
        return Ok(ApiResponse<CommentLikeResponseDto>.SuccessResponse(result));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (claim == null || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        return userId;
    }
}
