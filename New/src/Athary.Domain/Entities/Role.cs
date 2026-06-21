using MassTransit;
using Microsoft.AspNetCore.Identity;

namespace Athary.Domain.Entities;

public class Role : IdentityRole<Guid>
{
    public Role()
    {
        Id = NewId.NextSequentialGuid();
    }

    public Role(string roleName) : this()
    {
        Name = roleName;
        NormalizedName = roleName.ToUpper();
    }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public List<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
