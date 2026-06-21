using Athary.Application.Common;
using Athary.Application.DTOs.Communication;
using Athary.Application.Interfaces.Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers.Communication;

[ApiController]
[Route("api/system-settings")]
[Authorize(Roles = "Admin")]
public class SystemSettingsController : ControllerBase
{
    private readonly ISystemSettingService _systemSettingService;

    public SystemSettingsController(ISystemSettingService systemSettingService)
    {
        _systemSettingService = systemSettingService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SettingResponse>>>> GetAll([FromQuery] string? group = null, CancellationToken cancellationToken = default)
    {
        var result = await _systemSettingService.GetAllAsync(group, cancellationToken);
        return Ok(ApiResponse<List<SettingResponse>>.SuccessResponse(result));
    }

    [HttpGet("{key}")]
    public async Task<ActionResult<ApiResponse<SettingResponse>>> GetByKey(string key, CancellationToken cancellationToken)
    {
        var result = await _systemSettingService.GetByKeyAsync(key, cancellationToken);
        return Ok(ApiResponse<SettingResponse>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SettingResponse>>> Create([FromBody] CreateSettingRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _systemSettingService.CreateAsync(request, userId, cancellationToken);
        return CreatedAtAction(nameof(GetByKey), new { key = result.Key }, ApiResponse<SettingResponse>.SuccessResponse(result));
    }

    [HttpPut("{key}")]
    public async Task<ActionResult<ApiResponse<SettingResponse>>> Update(string key, [FromBody] UpdateSettingRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _systemSettingService.UpdateAsync(key, request, userId, cancellationToken);
        return Ok(ApiResponse<SettingResponse>.SuccessResponse(result));
    }

    [HttpDelete("{key}")]
    public async Task<IActionResult> Delete(string key, CancellationToken cancellationToken)
    {
        await _systemSettingService.DeleteAsync(key, cancellationToken);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (claim == null || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        return userId;
    }
}
