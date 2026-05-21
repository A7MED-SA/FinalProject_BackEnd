using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.DTOs.VideoComment;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

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
    public async Task<IActionResult> GetComments(Guid videoId)
    {
        var userId = GetUserId();
        var result = await _videoCommentService.GetCommentsForVideoAsync(videoId, userId);
        return Ok(ApiResponse<IEnumerable<CommentResponseDto>>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<IActionResult> AddComment(Guid videoId, [FromBody] CreateCommentDto createDto)
    {
        try
        {
            var userId = GetUserId();
            var result = await _videoCommentService.AddCommentAsync(videoId, userId, createDto);
            return CreatedAtAction(nameof(GetComments), new { videoId }, ApiResponse<CommentResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CommentResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{commentId}")]
    public async Task<IActionResult> UpdateComment(Guid videoId, Guid commentId, [FromBody] CreateCommentDto updateDto)
    {
        try
        {
            var userId = GetUserId();
            var result = await _videoCommentService.UpdateCommentAsync(commentId, userId, updateDto);
            return Ok(ApiResponse<CommentResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CommentResponseDto>.FailureResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<CommentResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{commentId}")]
    public async Task<IActionResult> DeleteComment(Guid videoId, Guid commentId)
    {
        var userId = GetUserId();
        var result = await _videoCommentService.DeleteCommentAsync(commentId, userId);
        if (!result)
            return NotFound(ApiResponse<object>.FailureResponse("Comment not found."));

        return Ok(ApiResponse<object>.SuccessResponse(new { message = "Deleted" }));
    }

    [HttpPost("{commentId}/like")]
    public async Task<IActionResult> ToggleLike(Guid videoId, Guid commentId)
    {
        try
        {
            var userId = GetUserId();
            var result = await _videoCommentService.ToggleLikeAsync(commentId, userId);
            return Ok(ApiResponse<CommentLikeResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CommentLikeResponseDto>.FailureResponse(ex.Message));
        }
    }

    private Guid GetUserId()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        return userId;
    }
}
