using System.Security.Claims;
using Athary.Application.Common;
using Athary.Application.DTOs.Auth;
using Athary.Application.Interfaces.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("Auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly ISessionService _sessionService;

    public AuthController(
        IAuthenticationService authService,
        ISessionService sessionService)
    {
        _authService = authService;
        _sessionService = sessionService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<RegisterResponseDto>>> Register(
        [FromBody] RegisterDto dto,
        CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var response = await _authService.RegisterAsync(dto, ipAddress, cancellationToken);
        return Ok(ApiResponse<RegisterResponseDto>.SuccessResponse(
            response, "Registration successful. Please check your email for verification code."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(
        [FromBody] LoginDto dto,
        CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
        var response = await _authService.LoginAsync(dto, ipAddress, userAgent, cancellationToken);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(response));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken(
        [FromBody] RefreshTokenDto dto,
        CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
        var response = await _authService.RefreshTokenAsync(dto.RefreshToken, ipAddress, userAgent, cancellationToken);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(response));
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult<ApiResponse<object>>> VerifyEmail(
        [FromBody] VerifyEmailDto dto,
        CancellationToken cancellationToken)
    {
        await _authService.VerifyEmailAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Email verified successfully. Your account is now active."));
    }

    [HttpPost("resend-verification")]
    public async Task<ActionResult<ApiResponse<object>>> ResendVerification(
        [FromBody] ResendVerificationDto dto,
        CancellationToken cancellationToken)
    {
        await _authService.ResendVerificationAsync(dto.Email, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Verification code sent to your email."));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword(
        [FromBody] ForgotPasswordDto dto,
        CancellationToken cancellationToken)
    {
        await _authService.RequestPasswordResetAsync(dto.Email, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "If the email exists, a password reset code has been sent."));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword(
        [FromBody] ResetPasswordDto dto,
        CancellationToken cancellationToken)
    {
        await _authService.ResetPasswordAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Password reset successfully. Please login with your new password."));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
        [FromBody] ChangePasswordDto dto,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return BadRequest(ApiResponse<object>.FailureResponse("Invalid user"));

        await _authService.ChangePasswordAsync(userId, dto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Password changed successfully"));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout(CancellationToken cancellationToken)
    {
        var sessionIdClaim = User.FindFirst("sid")?.Value;
        if (sessionIdClaim is null || !Guid.TryParse(sessionIdClaim, out var sessionId))
            return BadRequest(ApiResponse<object>.FailureResponse("Invalid session"));

        await _authService.LogoutAsync(sessionId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Logged out successfully"));
    }

    [Authorize]
    [HttpPost("logout-all")]
    public async Task<ActionResult<ApiResponse<object>>> LogoutAll(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var sessionIdClaim = User.FindFirst("sid")?.Value;

        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return BadRequest(ApiResponse<object>.FailureResponse("Invalid user"));

        if (sessionIdClaim is null || !Guid.TryParse(sessionIdClaim, out var currentSessionId))
            return BadRequest(ApiResponse<object>.FailureResponse("Invalid session"));

        await _authService.LogoutAllSessionsAsync(userId, currentSessionId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "All other sessions logged out successfully"));
    }

    [Authorize]
    [HttpGet("sessions")]
    public async Task<ActionResult<ApiResponse<List<SessionDto>>>> GetSessions(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return BadRequest(ApiResponse<List<SessionDto>>.FailureResponse("Invalid user"));

        var sessions = await _sessionService.GetUserSessionsAsync(userId, cancellationToken);
        return Ok(ApiResponse<List<SessionDto>>.SuccessResponse(sessions));
    }
}
