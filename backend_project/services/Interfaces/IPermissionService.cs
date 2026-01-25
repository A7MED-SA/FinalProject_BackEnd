namespace backend_project.Services.Interfaces;

public interface IPermissionService
{
    /// <summary>
    /// Retrieves all permission names for a user (aggregated from all roles)
    /// </summary>
    Task<List<string>> GetUserPermissionsAsync(Guid userId);

    /// <summary>
    /// Checks if a user has a specific permission
    /// </summary>
    Task<bool> HasPermissionAsync(Guid userId, string permissionName);
}
