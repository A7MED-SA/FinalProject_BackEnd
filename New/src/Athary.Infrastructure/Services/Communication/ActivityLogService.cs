using Athary.Application.Interfaces.Authentication;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Domain.Interfaces;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.Communication;

public sealed class ActivityLogService : IActivityLogService
{
    private readonly IRepository<ActivityLog> _activityLogRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public ActivityLogService(
        IRepository<ActivityLog> activityLogRepo,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
    {
        _activityLogRepo = activityLogRepo;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task LogActivityAsync(
        Guid userId,
        string action,
        string description,
        string ipAddress,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        var log = new ActivityLog
        {
            UserId = userId,
            Action = action,
            EntityType = ActivityLogEntityType.User,
            Details = description,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        await _activityLogRepo.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task LogDeletionAsync(
        Guid userId,
        ActivityLogEntityType entityType,
        Guid entityId,
        string details,
        CancellationToken cancellationToken = default)
    {
        var log = new ActivityLog
        {
            UserId = userId,
            Action = "Delete",
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            CreatedAt = DateTime.UtcNow
        };

        await _activityLogRepo.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IEnumerable<ActivityLog> Items, int TotalCount)> GetLogsAsync(
        Guid? userId = null,
        string? action = null,
        string? entityType = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string? ipAddress = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
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

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
