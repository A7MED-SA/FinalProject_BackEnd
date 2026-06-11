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

    Task<(IEnumerable<ActivityLog> Items, int TotalCount)> GetLogsAsync(
        Guid? userId = null,
        string? action = null,
        string? entityType = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string? ipAddress = null,
        int page = 1,
        int pageSize = 50);
}
