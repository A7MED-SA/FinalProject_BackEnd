namespace Athary.Application.DTOs.Auth;

public record OAuthLoginDto
{
    public string IdToken { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
}
