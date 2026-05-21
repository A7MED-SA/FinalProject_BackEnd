using backend_project.Models;

namespace backend_project.Services.Interfaces;

public interface IActivityLogService
{
    Task LogActivityAsync(
        Guid userId,
        string action,
        string description,
        string ipAddress,
        string? userAgent = null);

    Task LogActivityAsync(
        string action,
        string description,
        string ipAddress,
        string? userAgent = null);

    Task LogDeletionAsync(
        Guid userId,
        ActivityLogEntityType entityType,
        Guid entityId,
        string details);
}
