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
        // For activities without a user (e.g., failed login attempts)
        // We create a log without UserId - need to adjust the model to make UserId nullable
        // For now, we'll skip this overload or use a placeholder
        // In production, you might want to have a separate table for anonymous activity logs
        
        // Since the ActivityLog model requires UserId, we'll skip this for now
        // You can implement a separate logging mechanism or adjust the model
        await Task.CompletedTask;
    }
}
