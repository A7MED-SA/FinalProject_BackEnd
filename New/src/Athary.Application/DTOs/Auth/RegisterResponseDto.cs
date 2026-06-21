namespace Athary.Application.DTOs.Auth;

public record RegisterResponseDto
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
