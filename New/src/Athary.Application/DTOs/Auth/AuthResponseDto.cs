namespace Athary.Application.DTOs.Auth;

public record AuthResponseDto
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public Guid SessionId { get; init; }
    public DateTime ExpiresAt { get; init; }
    public UserInfoDto User { get; init; } = null!;
}
