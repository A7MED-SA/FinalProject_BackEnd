using backend_project.DTOs.Auth;
using backend_project.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend_project.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OAuthController : ControllerBase
{
    private readonly IOAuthService _oauthService;

    public OAuthController(IOAuthService oauthService)
    {
        _oauthService = oauthService;
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] OAuthLoginDto dto)
    {
        try
        {
            if (dto.Provider.ToLower() != "google")
                return BadRequest(new { error = "Invalid provider. Use 'google' for this endpoint." });

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            var response = await _oauthService.AuthenticateGoogleAsync(dto.IdToken, ipAddress, userAgent);
            
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("microsoft")]
    public async Task<IActionResult> MicrosoftLogin([FromBody] OAuthLoginDto dto)
    {
        try
        {
            if (dto.Provider.ToLower() != "microsoft")
                return BadRequest(new { error = "Invalid provider. Use 'microsoft' for this endpoint." });

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            var response = await _oauthService.AuthenticateMicrosoftAsync(dto.IdToken, ipAddress, userAgent);
            
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
