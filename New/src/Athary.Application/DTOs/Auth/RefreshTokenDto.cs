namespace Athary.Application.DTOs.Auth;

public record RefreshTokenDto
{
    public string RefreshToken { get; init; } = string.Empty;
}
