using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.Communication;
using backend_project.Models;
using backend_project.Services.Interfaces;
using backend_project.Services.Notifications;

namespace backend_project.Services.Implementations;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IActivityLogService _activityLogService;

    public ReportService(
        ApplicationDbContext context,
        INotificationService notificationService,
        IActivityLogService activityLogService)
    {
        _context = context;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
    }

    public async Task<ReportResponse> CreateAsync(Guid userId, CreateReportRequest request)
    {
        if (!Enum.TryParse<ReportEntityType>(request.EntityType, out var entityType))
            throw new InvalidOperationException($"Invalid entity type '{request.EntityType}'.");

        if (!Enum.TryParse<ReportReason>(request.Reason, out var reason))
            throw new InvalidOperationException($"Invalid reason '{request.Reason}'.");

        var existing = await _context.Reports
            .FirstOrDefaultAsync(r =>
                r.ReporterId == userId &&
                r.EntityType == entityType &&
                r.EntityId == request.EntityId &&
                r.Status == ReportStatus.Pending);

        if (existing != null)
        {
            existing.Description = request.Description ?? existing.Description;
            await _context.SaveChangesAsync();
            return MapToResponse(existing);
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
        await _context.SaveChangesAsync();

        return MapToResponse(report);
    }

    public async Task<IEnumerable<ReportResponse>> GetPendingAsync(int page, int pageSize)
    {
        var query = _context.Reports
            .Include(r => r.Reporter)
            .Where(r => r.Status == ReportStatus.Pending)
            .OrderByDescending(r => r.CreatedAt);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return items.Select(MapToResponse);
    }

    public async Task<ReportResponse> ResolveAsync(Guid reportId, ResolveReportRequest request, Guid adminId)
    {
        var report = await _context.Reports
            .Include(r => r.Reporter)
            .FirstOrDefaultAsync(r => r.Id == reportId)
            ?? throw new KeyNotFoundException("Report not found.");

        if (!Enum.TryParse<ReportStatus>(request.Status, out var status))
            throw new InvalidOperationException($"Invalid status '{request.Status}'.");

        if (status != ReportStatus.Dismissed && status != ReportStatus.ActionTaken)
            throw new InvalidOperationException("Status must be Dismissed or ActionTaken.");

        report.Status = status;
        report.AdminNote = request.AdminNote;
        report.ResolvedAt = DateTime.UtcNow;
        report.ResolvedBy = adminId;

        await _context.SaveChangesAsync();

        var outcome = status == ReportStatus.ActionTaken ? "action was taken" : "was dismissed";
        await _notificationService.CreateNotificationAsync(
            report.ReporterId,
            "Report Resolved",
            $"Your report regarding {report.EntityType} {outcome}.",
            NotificationType.System);

        await _activityLogService.LogActivityAsync(
            adminId, "ReportResolved",
            $"Report {reportId} resolved as {status}",
            $"{{ \"reportId\": \"{reportId}\", \"status\": \"{status}\", \"entityType\": \"{report.EntityType}\" }}",
            null);

        return MapToResponse(report);
    }

    private static ReportResponse MapToResponse(Report r)
    {
        return new ReportResponse
        {
            Id = r.Id,
            ReporterId = r.ReporterId,
            ReporterName = $"{r.Reporter.FirstName} {r.Reporter.LastName}",
            EntityType = r.EntityType.ToString(),
            EntityId = r.EntityId,
            Reason = r.Reason.ToString(),
            Description = r.Description,
            Status = r.Status.ToString(),
            AdminNote = r.AdminNote,
            CreatedAt = r.CreatedAt,
            ResolvedAt = r.ResolvedAt
        };
    }
}
