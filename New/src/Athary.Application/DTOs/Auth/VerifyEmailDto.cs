namespace Athary.Application.DTOs.Auth;

public record VerifyEmailDto
{
    public string Token { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
