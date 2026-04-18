using backend_project.Data;
using backend_project.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_project.Services;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;

    public PermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        var permissions = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_context.RolePermissions,
                ur => ur.RoleId,
                rp => rp.RoleId,
                (ur, rp) => rp.PermissionId)
            .Join(_context.Permissions,
                permissionId => permissionId,
                p => p.Id,
                (permissionId, p) => p.Name)
            .Distinct()
            .ToListAsync();

        return permissions;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permissionName)
    {
        var hasPermission = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_context.RolePermissions,
                ur => ur.RoleId,
                rp => rp.RoleId,
                (ur, rp) => rp.PermissionId)
            .Join(_context.Permissions,
                permissionId => permissionId,
                p => p.Id,
                (permissionId, p) => p.Name)
            .AnyAsync(name => name == permissionName);

        return hasPermission;
    }
}
