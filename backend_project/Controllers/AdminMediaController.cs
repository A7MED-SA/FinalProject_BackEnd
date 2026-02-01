using backend_project.DTOs.Media;
using backend_project.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend_project.Controllers;

[ApiController]
[Route("api/admin/media")]
[Authorize(Roles = "Admin")]
public class AdminMediaController : ControllerBase
{
    private readonly IAdminMediaService _adminMediaService;
    private readonly ILogger<AdminMediaController> _logger;

    public AdminMediaController(
        IAdminMediaService adminMediaService,
        ILogger<AdminMediaController> logger)
    {
        _adminMediaService = adminMediaService;
        _logger = logger;
    }

    /// <summary>
    /// List media files with filters
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListMedia([FromQuery] MediaFilterDto filter)
    {
        try
        {
            var result = await _adminMediaService.ListMediaAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing media");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get detailed media file info
    /// </summary>
    [HttpGet("{fileId}")]
    public async Task<IActionResult> GetMediaDetails(Guid fileId)
    {
        try
        {
            var result = await _adminMediaService.GetMediaDetailsAsync(fileId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media details for {FileId}", fileId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Soft delete a media file
    /// </summary>
    [HttpDelete("{fileId}")]
    public async Task<IActionResult> SoftDelete(Guid fileId)
    {
        try
        {
            var adminId = GetAdminId();
            await _adminMediaService.SoftDeleteAsync(fileId, adminId);
            return Ok(new { message = "File soft deleted successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting {FileId}", fileId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Restore a soft-deleted media file
    /// </summary>
    [HttpPost("{fileId}/restore")]
    public async Task<IActionResult> Restore(Guid fileId)
    {
        try
        {
            var adminId = GetAdminId();
            await _adminMediaService.RestoreAsync(fileId, adminId);
            return Ok(new { message = "File restored successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring {FileId}", fileId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Permanently delete from DB and MinIO (DANGEROUS)
    /// </summary>
    [HttpDelete("{fileId}/permanent")]
    public async Task<IActionResult> PermanentDelete(Guid fileId)
    {
        try
        {
            var adminId = GetAdminId();
            _logger.LogWarning("Admin {AdminId} initiating permanent delete of {FileId}", adminId, fileId);

            await _adminMediaService.PermanentDeleteAsync(fileId, adminId);
            return Ok(new { message = "File permanently deleted" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error permanently deleting {FileId}", fileId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get storage statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStorageStats()
    {
        try
        {
            var result = await _adminMediaService.GetStorageStatsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting storage stats");
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetAdminId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Admin not authenticated");
        return Guid.Parse(userIdClaim);
    }
}
