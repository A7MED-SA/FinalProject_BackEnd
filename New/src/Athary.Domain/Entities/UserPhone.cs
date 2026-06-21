using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class UserPhone : BaseEntity
{
    public Guid UserId { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public PhoneType Type { get; set; } = PhoneType.Primary;

    public bool IsVerified { get; set; } = false;

    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
