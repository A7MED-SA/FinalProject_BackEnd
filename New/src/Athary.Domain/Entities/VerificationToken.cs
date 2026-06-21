using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class VerificationToken : BaseEntity
{
    public VerificationToken()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; set; }

    public string Token { get; set; } = string.Empty;

    public string TokenHash { get; set; } = string.Empty;

    public VerificationTokenType TokenType { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? IpAddress { get; set; }

    public User User { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public bool IsUsed => UsedAt.HasValue;

    public bool IsValid => !IsExpired && !IsUsed;
}
