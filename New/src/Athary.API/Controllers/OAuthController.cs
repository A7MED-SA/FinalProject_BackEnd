using Athary.Application.Common;
using Athary.Application.DTOs.Auth;
using Athary.Application.Interfaces.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/oauth")]
public class OAuthController : ControllerBase
{
    private readonly IOAuthService _oauthService;

    public OAuthController(IOAuthService oauthService)
    {
        _oauthService = oauthService;
    }

    [HttpPost("google")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> GoogleLogin(
        [FromBody] OAuthLoginDto dto,
        CancellationToken cancellationToken)
    {
        if (dto.Provider.Equals("google", StringComparison.OrdinalIgnoreCase) is false)
            return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse("Invalid provider. Use 'google' for this endpoint."));

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
        var response = await _oauthService.AuthenticateGoogleAsync(dto.IdToken, ipAddress, userAgent, cancellationToken);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(response));
    }

    [HttpPost("microsoft")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> MicrosoftLogin(
        [FromBody] OAuthLoginDto dto,
        CancellationToken cancellationToken)
    {
        if (dto.Provider.Equals("microsoft", StringComparison.OrdinalIgnoreCase) is false)
            return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse("Invalid provider. Use 'microsoft' for this endpoint."));

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
        var response = await _oauthService.AuthenticateMicrosoftAsync(dto.IdToken, ipAddress, userAgent, cancellationToken);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(response));
    }
}
