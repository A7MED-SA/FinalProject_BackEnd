namespace Athary.Domain.Entities;

public sealed class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Resource { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
