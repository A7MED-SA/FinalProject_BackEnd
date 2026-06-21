using System.Security.Claims;
using Athary.Application.Common;
using Athary.Application.DTOs.Media;
using Athary.Application.Interfaces.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/admin/media")]
[Authorize(Roles = "Admin")]
public class AdminMediaController : ControllerBase
{
    private readonly IAdminMediaService _adminMediaService;

    public AdminMediaController(IAdminMediaService adminMediaService)
    {
        _adminMediaService = adminMediaService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<MediaFileDto>>>> ListMedia(
        [FromQuery] MediaFilterDto filter,
        CancellationToken cancellationToken)
    {
        var result = await _adminMediaService.ListMediaAsync(filter, cancellationToken);
        return Ok(ApiResponse<PagedResult<MediaFileDto>>.SuccessResponse(result));
    }

    [HttpGet("{fileId:guid}")]
    public async Task<ActionResult<ApiResponse<MediaDetailDto>>> GetMediaDetails(
        Guid fileId,
        CancellationToken cancellationToken)
    {
        var result = await _adminMediaService.GetMediaDetailsAsync(fileId, cancellationToken);
        return Ok(ApiResponse<MediaDetailDto>.SuccessResponse(result));
    }

    [HttpDelete("{fileId:guid}/soft")]
    public async Task<ActionResult<ApiResponse<object>>> SoftDelete(
        Guid fileId,
        CancellationToken cancellationToken)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (adminId is null) return Unauthorized();

        await _adminMediaService.SoftDeleteAsync(fileId, Guid.Parse(adminId), cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "File soft deleted"));
    }

    [HttpPost("{fileId:guid}/restore")]
    public async Task<ActionResult<ApiResponse<object>>> Restore(
        Guid fileId,
        CancellationToken cancellationToken)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (adminId is null) return Unauthorized();

        await _adminMediaService.RestoreAsync(fileId, Guid.Parse(adminId), cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "File restored"));
    }

    [HttpDelete("{fileId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> PermanentDelete(
        Guid fileId,
        CancellationToken cancellationToken)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (adminId is null) return Unauthorized();

        await _adminMediaService.PermanentDeleteAsync(fileId, Guid.Parse(adminId), cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "File permanently deleted"));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<StorageStatsDto>>> GetStats(CancellationToken cancellationToken)
    {
        var result = await _adminMediaService.GetStorageStatsAsync(cancellationToken);
        return Ok(ApiResponse<StorageStatsDto>.SuccessResponse(result));
    }
}
