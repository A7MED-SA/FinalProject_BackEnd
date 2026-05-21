using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.DTOs.Video;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/courses/{courseId}/videos")]
[Authorize(Roles = "Instructor")]
public class VideoContentController : ControllerBase
{
    private readonly IVideoContentService _videoContentService;

    public VideoContentController(IVideoContentService videoContentService)
    {
        _videoContentService = videoContentService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVideo(Guid courseId, Guid id)
    {
        try
        {
            var result = await _videoContentService.GetVideoAsync(id);
            return Ok(ApiResponse<VideoResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<VideoResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateVideo(Guid courseId, [FromBody] CreateVideoDto createDto)
    {
        try
        {
            var result = await _videoContentService.CreateVideoAsync(createDto);
            return CreatedAtAction(nameof(GetVideo), new { courseId, id = result.Id }, ApiResponse<VideoResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<VideoResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVideo(Guid courseId, Guid id, [FromBody] UpdateVideoDto updateDto)
    {
        try
        {
            var result = await _videoContentService.UpdateVideoAsync(id, updateDto);
            return Ok(ApiResponse<VideoResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<VideoResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVideo(Guid courseId, Guid id)
    {
        var result = await _videoContentService.DeleteVideoAsync(id);
        if (!result)
            return NotFound(ApiResponse<object>.FailureResponse("Video not found."));

        return Ok(ApiResponse<object>.SuccessResponse(new { message = "Deleted" }));
    }

    private Guid GetUserId()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        return userId;
    }
}
