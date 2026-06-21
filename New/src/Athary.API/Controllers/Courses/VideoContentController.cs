using Athary.Application.Common;
using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Athary.API.Controllers.Courses;

[ApiController]
[Route("api/courses/{courseId:guid}/videos")]
[Authorize(Roles = "Instructor")]
public class VideoContentController : ControllerBase
{
    private readonly IVideoService _videoService;

    public VideoContentController(IVideoService videoService)
    {
        _videoService = videoService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<VideoResponseDto>>> GetVideo(
        Guid courseId,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _videoService.GetVideoAsync(id, cancellationToken);
            return Ok(ApiResponse<VideoResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<VideoResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<VideoResponseDto>>> CreateVideo(
        Guid courseId,
        [FromBody] CreateVideoDto createDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _videoService.CreateVideoAsync(courseId, createDto, cancellationToken);
            return CreatedAtAction(nameof(GetVideo), new { courseId, id = result.Id }, ApiResponse<VideoResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<VideoResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<VideoResponseDto>>> UpdateVideo(
        Guid courseId,
        Guid id,
        [FromBody] UpdateVideoDto updateDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _videoService.UpdateVideoAsync(id, updateDto, cancellationToken);
            return Ok(ApiResponse<VideoResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<VideoResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteVideo(
        Guid courseId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _videoService.DeleteVideoAsync(id, cancellationToken);
        if (!result)
            return NotFound(ApiResponse<object>.FailureResponse("Video not found."));

        return Ok(ApiResponse<object>.SuccessResponse(new { message = "Deleted" }));
    }
}
