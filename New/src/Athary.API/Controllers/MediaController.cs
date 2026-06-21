using System.Security.Claims;
using Athary.Application.Common;
using Athary.Application.DTOs.Media;
using Athary.Application.Interfaces.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/media")]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public MediaController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [Authorize]
    [HttpPost("upload-url")]
    public async Task<ActionResult<ApiResponse<UploadUrlResponseDto>>> GenerateUploadUrl(
        [FromBody] UploadUrlRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var result = await _mediaService.GenerateUploadUrlAsync(Guid.Parse(userId), request, cancellationToken);
        return Ok(ApiResponse<UploadUrlResponseDto>.SuccessResponse(result));
    }

    [Authorize]
    [HttpPost("confirm-upload")]
    public async Task<ActionResult<ApiResponse<MediaFileDto>>> ConfirmUpload(
        [FromBody] ConfirmUploadDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var result = await _mediaService.ConfirmUploadAsync(Guid.Parse(userId), request, cancellationToken);
        return Ok(ApiResponse<MediaFileDto>.SuccessResponse(result));
    }

    [AllowAnonymous]
    [HttpGet("{fileId:guid}/view-url")]
    public async Task<ActionResult<ApiResponse<ViewUrlResponseDto>>> GetViewUrl(
        Guid fileId,
        [FromQuery] Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var userRoles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value);

        var result = await _mediaService.GetViewUrlAsync(fileId, userId, userRoles, cancellationToken);
        return Ok(ApiResponse<ViewUrlResponseDto>.SuccessResponse(result));
    }
}
