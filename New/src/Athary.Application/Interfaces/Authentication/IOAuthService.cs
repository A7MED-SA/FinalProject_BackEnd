using Athary.Application.DTOs.Auth;

namespace Athary.Application.Interfaces.Authentication;

public interface IOAuthService
{
    Task<AuthResponseDto> AuthenticateGoogleAsync(string idToken, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> AuthenticateMicrosoftAsync(string idToken, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
}
