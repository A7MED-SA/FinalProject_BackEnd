namespace backend_project.DTOs.Auth;

public record SessionDto
{
    public Guid Id { get; init; }
    public string IpAddress { get; init; } = string.Empty;
    public string UserAgent { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? LastUsed { get; init; }
    public bool IsActive { get; init; }
}
