using backend_project.DTOs.Auth;

namespace backend_project.Services.Interfaces;

public interface IOAuthService
{
    /// <summary>
    /// Authenticates a user with Google OAuth ID token
    /// </summary>
    Task<AuthResponseDto> AuthenticateGoogleAsync(string idToken, string ipAddress, string userAgent);

    /// <summary>
    /// Authenticates a user with Microsoft OAuth ID token
    /// </summary>
    Task<AuthResponseDto> AuthenticateMicrosoftAsync(string idToken, string ipAddress, string userAgent);
}
