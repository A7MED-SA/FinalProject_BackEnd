using Microsoft.AspNetCore.Identity;

namespace Athary.Domain.Entities;

public class UserRole : IdentityUserRole<Guid>
{
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    public User User { get; set; } = null!;

    public Role Role { get; set; } = null!;
}
