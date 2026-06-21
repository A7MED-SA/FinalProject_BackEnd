using Athary.Application.DTOs.Communication;
using Athary.Application.Interfaces.Authentication;
using Athary.Application.Interfaces.Communication;
using Athary.Application.Interfaces.Notification;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Data;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.Communication;

public sealed class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IActivityLogService _activityLogService;
    private readonly IMapper _mapper;

    public ReportService(
        ApplicationDbContext context,
        INotificationService notificationService,
        IActivityLogService activityLogService,
        IMapper mapper)
    {
        _context = context;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
        _mapper = mapper;
    }

    public async Task<ReportResponse> CreateAsync(Guid userId, CreateReportRequest request, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<ReportEntityType>(request.EntityType, true, out var entityType))
            throw new InvalidOperationException($"نوع الكيان '{request.EntityType}' غير صالح.");

        if (!Enum.TryParse<ReportReason>(request.Reason, true, out var reason))
            throw new InvalidOperationException($"السبب '{request.Reason}' غير صالح.");

        var existing = await _context.Reports
            .FirstOrDefaultAsync(r =>
                r.ReporterId == userId &&
                r.EntityType == entityType &&
                r.EntityId == request.EntityId &&
                r.Status == ReportStatus.Pending, cancellationToken);

        if (existing is not null)
        {
            existing.Description = request.Description ?? existing.Description;
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<ReportResponse>(existing);
        }

        var report = new Report
        {
            ReporterId = userId,
            EntityType = entityType,
            EntityId = request.EntityId,
            Reason = reason,
            Description = request.Description,
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync(cancellationToken);

        await _context.Entry(report).Reference(r => r.Reporter).LoadAsync(cancellationToken);
        return _mapper.Map<ReportResponse>(report);
    }

    public async Task<ReportListResponse> GetPendingAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _context.Reports
            .Include(r => r.Reporter)
            .Where(r => r.Status == ReportStatus.Pending);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new ReportListResponse
        {
            Items = _mapper.Map<List<ReportResponse>>(items),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ReportResponse> ResolveAsync(Guid reportId, ResolveReportRequest request, Guid adminId, CancellationToken cancellationToken = default)
    {
        var report = await _context.Reports
            .Include(r => r.Reporter)
            .FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken)
            ?? throw new KeyNotFoundException("البلاغ غير موجود.");

        if (!Enum.TryParse<ReportStatus>(request.Status, true, out var status))
            throw new InvalidOperationException($"الحالة '{request.Status}' غير صالحة.");

        if (status != ReportStatus.Dismissed && status != ReportStatus.ActionTaken)
            throw new InvalidOperationException("يجب أن تكون الحالة Dismissed أو ActionTaken.");

        report.Status = status;
        report.AdminNote = request.AdminNote;
        report.ResolvedAt = DateTime.UtcNow;
        report.ResolvedBy = adminId;

        await _context.SaveChangesAsync(cancellationToken);

        var outcome = status == ReportStatus.ActionTaken ? "تم اتخاذ إجراء" : "تم الرفض";
        await _notificationService.CreateNotificationAsync(
            report.ReporterId,
            "تم معالجة البلاغ",
            $"تم معالجة بلاغك بخصوص {report.EntityType}: {outcome}.",
            NotificationType.System,
            cancellationToken: cancellationToken);

        await _activityLogService.LogActivityAsync(
            adminId, "ReportResolved",
            $"Report {reportId} resolved as {status}",
            "system", cancellationToken: cancellationToken);

        return _mapper.Map<ReportResponse>(report);
    }

}
