using Microsoft.EntityFrameworkCore;
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

    public async Task<(IEnumerable<ActivityLog> Items, int TotalCount)> GetLogsAsync(
        Guid? userId = null,
        string? action = null,
        string? entityType = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string? ipAddress = null,
        int page = 1,
        int pageSize = 50)
    {
        var query = _context.ActivityLogs
            .Include(l => l.User)
            .AsQueryable();

        if (userId.HasValue)
            query = query.Where(l => l.UserId == userId.Value);
        if (!string.IsNullOrEmpty(action))
            query = query.Where(l => l.Action == action);
        if (!string.IsNullOrEmpty(entityType) && Enum.TryParse<ActivityLogEntityType>(entityType, out var et))
            query = query.Where(l => l.EntityType == et);
        if (dateFrom.HasValue)
            query = query.Where(l => l.CreatedAt >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(l => l.CreatedAt <= dateTo.Value);
        if (!string.IsNullOrEmpty(ipAddress))
            query = query.Where(l => l.IpAddress != null && l.IpAddress.Contains(ipAddress));

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
