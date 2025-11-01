using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("role_permissions")]
public class RolePermission : BaseEntity
{

    [Required]
    [Column("role_id")]
    public Guid RoleId { get; set; }

    [Required]
    [Column("permission_id")]
    public Guid PermissionId { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("granted_at")]
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("RoleId")]
    public virtual Role Role { get; set; } = null!;

    [ForeignKey("PermissionId")]
    public virtual Permission Permission { get; set; } = null!;
}
