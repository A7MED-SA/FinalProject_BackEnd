using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend_project.DTOs;
using backend_project.DTOs.Communication;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers.Communication;

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

    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? group = null)
    {
        var result = await _systemSettingService.GetAllAsync(group);
        return Ok(ApiResponse<IEnumerable<SettingResponse>>.SuccessResponse(result));
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(string key)
    {
        try
        {
            var result = await _systemSettingService.GetByKeyAsync(key);
            return Ok(ApiResponse<SettingResponse>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{key}")]
    public async Task<IActionResult> Update(string key, [FromBody] UpdateSettingRequest request)
    {
        try
        {
            var result = await _systemSettingService.UpdateAsync(key, request, GetUserId());
            return Ok(ApiResponse<SettingResponse>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSettingRequest request)
    {
        try
        {
            var result = await _systemSettingService.CreateAsync(request, GetUserId());
            return CreatedAtAction(nameof(GetByKey), new { key = result.Key }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{key}")]
    public async Task<IActionResult> Delete(string key)
    {
        try
        {
            await _systemSettingService.DeleteAsync(key);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }
}
