using Athary.Application.DTOs.Auth;

namespace Athary.Application.Interfaces.Authentication;

public interface IAuthenticationService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterDto dto, string ipAddress, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    Task VerifyEmailAsync(VerifyEmailDto dto, CancellationToken cancellationToken = default);
    Task ResendVerificationAsync(string email, CancellationToken cancellationToken = default);
    Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default);
    Task LogoutAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task LogoutAllSessionsAsync(Guid userId, Guid currentSessionId, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto, CancellationToken cancellationToken = default);
}
