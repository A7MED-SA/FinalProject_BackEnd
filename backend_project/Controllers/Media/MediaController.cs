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
            // Validate request
            if (request == null)
                return BadRequest(new { error = "Invalid request data" });

            if (string.IsNullOrWhiteSpace(request.FileName))
                return BadRequest(new { error = "File name is required" });

            if (request.FileSizeBytes <= 0 || request.FileSizeBytes > 5L * 1024 * 1024 * 1024) // 5GB max
                return BadRequest(new { error = "Invalid file size" });

            var userId = GetUserId();
            var result = await _mediaService.GenerateUploadUrlAsync(userId, request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to generate upload URL");
            return Unauthorized(new { error = "Authentication required" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid upload request");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating upload URL for user {UserId}", GetUserIdOrNull());
            return StatusCode(500, new { error = "Failed to generate upload URL. Please try again." });
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
            if (request == null)
                return BadRequest(new { error = "Invalid request data" });

            var userId = GetUserId();
            var result = await _mediaService.ConfirmUploadAsync(userId, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "File not found during confirmation: {FileId}", request?.FileId);
            return NotFound(new { error = "File not found" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during upload confirmation: {FileId}", request?.FileId);
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized upload confirmation attempt: {FileId}", request?.FileId);
            return StatusCode(403, new { error = "Access denied" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming upload for file {FileId}", request?.FileId);
            return StatusCode(500, new { error = "Failed to confirm upload. Please try again." });
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
            // Validate fileId to prevent injection attacks
            if (fileId == Guid.Empty)
                return BadRequest(new { error = "Invalid file identifier" });

            var userId = GetUserIdOrNull();
            var roles = GetUserRoles();

            var result = await _mediaService.GetViewUrlAsync(fileId, userId, roles);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "File not found for viewing: {FileId}", fileId);
            return NotFound(new { error = "File not found" });
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Access denied to file: {FileId} for user {UserId}", fileId, GetUserIdOrNull());
            return StatusCode(403, new { error = "Access denied" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation for file: {FileId}", fileId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting view URL for file {FileId}", fileId);
            return StatusCode(500, new { error = "Failed to generate view URL. Please try again." });
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid authentication token");
        }
        return userId;
    }

    private Guid? GetUserIdOrNull()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private IEnumerable<string> GetUserRoles()
    {
        return User.FindAll(ClaimTypes.Role).Select(c => c.Value);
    }
}