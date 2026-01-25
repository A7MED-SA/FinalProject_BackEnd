using backend_project.DTOs.Auth;
using backend_project.DTOs;
using backend_project.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend_project.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<IActionResult> Register([FromForm] RegisterDto dto, IFormFile? profilePicture = null)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            var response = await _authService.RegisterAsync(dto, ipAddress, profilePicture);
            
            return Ok(new
            {
                message = "Registration successful. Please check your email for verification code.",
                user = response
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            
            var response = await _authService.LoginAsync(dto, ipAddress, userAgent);
            
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

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            
            var response = await _authService.RefreshTokenAsync(dto.RefreshToken, ipAddress, userAgent);
            
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

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
    {
        try
        {
            await _authService.VerifyEmailAsync(dto);
            return Ok(new { message = "Email verified successfully. Your account is now active." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationDto dto)
    {
        try
        {
            await _authService.ResendVerificationAsync(dto.Email);
            return Ok(new { message = "Verification code sent to your email." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        try
        {
            await _authService.RequestPasswordResetAsync(dto.Email);
            return Ok(new { message = "If the email exists, a password reset code has been sent." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        try
        {
            await _authService.ResetPasswordAsync(dto);
            return Ok(new { message = "Password reset successfully. Please login with your new password." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var sessionIdClaim = User.FindFirst("sid")?.Value;
            if (sessionIdClaim == null || !Guid.TryParse(sessionIdClaim, out var sessionId))
                return BadRequest(new { error = "Invalid session" });

            await _authService.LogoutAsync(sessionId);
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAll()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var sessionIdClaim = User.FindFirst("sid")?.Value;

            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return BadRequest(new { error = "Invalid user" });

            if (sessionIdClaim == null || !Guid.TryParse(sessionIdClaim, out var currentSessionId))
                return BadRequest(new { error = "Invalid session" });

            await _authService.LogoutAllSessionsAsync(userId, currentSessionId);
            return Ok(new { message = "All other sessions logged out successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return BadRequest(new { error = "Invalid user" });

            var sessions = await _sessionService.GetUserSessionsAsync(userId);
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
