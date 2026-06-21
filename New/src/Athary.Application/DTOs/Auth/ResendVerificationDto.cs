namespace Athary.Application.DTOs.Auth;

public record ResendVerificationDto
{
    public string Email { get; init; } = string.Empty;
}
