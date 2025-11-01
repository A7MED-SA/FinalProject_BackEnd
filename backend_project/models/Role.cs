using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using MassTransit;

namespace backend_project.Models;

[Table("roles")]
public class Role : IdentityRole<Guid>
{
    public Role()
    {
        // Generate Sequential GUID for new roles
        Id = NewId.NextSequentialGuid();
    }
    
    public Role(string roleName) : this()
    {
        Name = roleName;
        NormalizedName = roleName.ToUpper();
    }

    // Name and NormalizedName are in IdentityRole

    [Column("description")]
    public string? Description { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
