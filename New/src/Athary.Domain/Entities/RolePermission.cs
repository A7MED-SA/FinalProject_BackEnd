namespace Athary.Domain.Entities;

public sealed class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }

    public Guid PermissionId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    public Role Role { get; set; } = null!;

    public Permission Permission { get; set; } = null!;
}
