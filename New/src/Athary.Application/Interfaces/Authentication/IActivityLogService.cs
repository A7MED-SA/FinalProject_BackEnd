using Athary.Domain.Entities;
using Athary.Domain.Enums;

namespace Athary.Application.Interfaces.Authentication;

public interface IActivityLogService
{
    Task LogActivityAsync(
        Guid userId,
        string action,
        string description,
        string ipAddress,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    Task LogDeletionAsync(
        Guid userId,
        ActivityLogEntityType entityType,
        Guid entityId,
        string details,
        CancellationToken cancellationToken = default);

    Task<(IEnumerable<ActivityLog> Items, int TotalCount)> GetLogsAsync(
        Guid? userId = null,
        string? action = null,
        string? entityType = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string? ipAddress = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
}
