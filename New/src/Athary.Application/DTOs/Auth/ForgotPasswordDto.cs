namespace Athary.Application.DTOs.Auth;

public record ForgotPasswordDto
{
    public string Email { get; init; } = string.Empty;
}
