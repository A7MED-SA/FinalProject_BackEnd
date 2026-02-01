using backend_project.DTOs.Media;
using backend_project.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend_project.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;
    private readonly ILogger<MediaController> _logger;

    public MediaController(
        IMediaService mediaService,
        ILogger<MediaController> logger)
    {
        _mediaService = mediaService;
        _logger = logger;
    }

    /// <summary>
    /// Generate a presigned upload URL
    /// </summary>
    [Authorize]
    [HttpPost("upload-url")]
    public async Task<IActionResult> GenerateUploadUrl([FromBody] UploadUrlRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _mediaService.GenerateUploadUrlAsync(userId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating upload URL");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Confirm upload completion
    /// </summary>
    [Authorize]
    [HttpPost("confirm-upload")]
    public async Task<IActionResult> ConfirmUpload([FromBody] ConfirmUploadDto request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _mediaService.ConfirmUploadAsync(userId, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming upload");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get a presigned view URL for a file
    /// </summary>
    [HttpGet("{fileId}/view")]
    public async Task<IActionResult> GetViewUrl(Guid fileId)
    {
        try
        {
            var userId = GetUserIdOrNull();
            var roles = GetUserRoles();

            var result = await _mediaService.GetViewUrlAsync(fileId, userId, roles);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, new { error = "Access denied" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting view URL for file {FileId}", fileId);
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(userIdClaim);
    }

    private Guid? GetUserIdOrNull()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim != null ? Guid.Parse(userIdClaim) : null;
    }

    private IEnumerable<string> GetUserRoles()
    {
        return User.FindAll(ClaimTypes.Role).Select(c => c.Value);
    }
}
