namespace backend_project.DTOs.Auth;

public record UserInfoDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? ProfilePictureUrl { get; init; }
    public bool IsActive { get; init; }
    public bool EmailConfirmed { get; init; }
    public List<string> Roles { get; init; } = new();
}
