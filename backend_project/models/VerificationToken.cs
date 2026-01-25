using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MassTransit;

namespace backend_project.Models;

[Table("verification_tokens")]
public class VerificationToken
{
    public VerificationToken()
    {
        Id = NewId.NextSequentialGuid();
        CreatedAt = DateTime.UtcNow;
    }

    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    [Required]
    public Guid UserId { get; set; }

    [Column("token")]
    [MaxLength(255)]
    [Required]
    public string Token { get; set; } = string.Empty;

    [Column("token_hash")]
    [MaxLength(500)]
    [Required]
    public string TokenHash { get; set; } = string.Empty;

    [Column("token_type")]
    [Required]
    public VerificationTokenType TokenType { get; set; }

    [Column("expires_at")]
    [Required]
    public DateTime ExpiresAt { get; set; }

    [Column("used_at")]
    public DateTime? UsedAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("ip_address")]
    [MaxLength(45)] // IPv6 support
    public string? IpAddress { get; set; }

    // Navigation Property
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    // Helper properties
    [NotMapped]
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    [NotMapped]
    public bool IsUsed => UsedAt.HasValue;

    [NotMapped]
    public bool IsValid => !IsExpired && !IsUsed;
}

public enum VerificationTokenType
{
    EmailVerification = 1,
    PasswordReset = 2,
    AccountActivation = 3,
    TwoFactorAuth = 4
}