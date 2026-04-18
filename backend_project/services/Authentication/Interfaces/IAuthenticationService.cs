using backend_project.DTOs.Auth;
using backend_project.DTOs;

namespace backend_project.Services.Interfaces;

public interface IAuthenticationService
{
    /// <summary>
    /// Registers a new user with email/password
    /// </summary>
    Task<RegisterResponseDto> RegisterAsync(RegisterDto dto, string ipAddress);

    /// <summary>
    /// Authenticates user with email and password
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginDto dto, string ipAddress, string userAgent);

    /// <summary>
    /// Refreshes access token using refresh token (with rotation)
    /// </summary>
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress, string userAgent);

    /// <summary>
    /// Verifies user email with OTP and activates account
    /// </summary>
    Task VerifyEmailAsync(VerifyEmailDto dto);

    /// <summary>
    /// Resends email verification OTP
    /// </summary>
    Task ResendVerificationAsync(string email);

    /// <summary>
    /// Initiates password reset by sending OTP
    /// </summary>
    Task RequestPasswordResetAsync(string email);

    /// <summary>
    /// Resets password using OTP
    /// </summary>
    Task ResetPasswordAsync(ResetPasswordDto dto);

    /// <summary>
    /// Logs out current session
    /// </summary>
    Task LogoutAsync(Guid sessionId);

    /// <summary>
    /// Logs out all sessions for a user except current one
    /// </summary>
    Task LogoutAllSessionsAsync(Guid userId, Guid currentSessionId);

    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);

}
