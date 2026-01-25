using backend_project.Models;

namespace backend_project.Services.Interfaces;

public interface IActivityLogService
{
    /// <summary>
    /// Logs a security-relevant activity for a user
    /// </summary>
    Task LogActivityAsync(
        Guid userId,
        string action,
        string description,
        string ipAddress,
        string? userAgent = null);

    /// <summary>
    /// Logs activity without requiring a user ID (for failed logins, etc.)
    /// </summary>
    Task LogActivityAsync(
        string action,
        string description,
        string ipAddress,
        string? userAgent = null);
}
