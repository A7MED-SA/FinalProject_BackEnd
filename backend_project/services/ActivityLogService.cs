using backend_project.Data;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly ApplicationDbContext _context;

    public ActivityLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogActivityAsync(
        Guid userId,
        string action,
        string description,
        string ipAddress,
        string? userAgent = null)
    {
        var log = new ActivityLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityType = ActivityLogEntityType.User,
            Details = description,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task LogActivityAsync(
        string action,
        string description,
        string ipAddress,
        string? userAgent = null)
    {
        await Task.CompletedTask;
    }

    public async Task LogDeletionAsync(
        Guid userId,
        ActivityLogEntityType entityType,
        Guid entityId,
        string details)
    {
        var log = new ActivityLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = "Delete",
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            CreatedAt = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
