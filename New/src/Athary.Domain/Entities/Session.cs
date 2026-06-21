namespace Athary.Domain.Entities;

public sealed class Session : BaseEntity
{
    public Guid UserId { get; set; }

    public string RefreshTokenHash { get; set; } = string.Empty;

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public DateTime RefreshExpiresAt { get; set; }

    public DateTime? LastActivityAt { get; set; }

    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;
}
