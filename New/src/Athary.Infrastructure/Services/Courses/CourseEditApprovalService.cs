using System.Text.Json;
using Athary.Application.Common;
using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Authentication;
using Athary.Application.Interfaces.Courses;
using Athary.Application.Interfaces.Notification;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Athary.Infrastructure.Services.Courses;

public sealed class CourseEditApprovalService : ICourseEditApprovalService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly IActivityLogService _activityLogService;
    private readonly ILogger<CourseEditApprovalService> _logger;

    public CourseEditApprovalService(
        ApplicationDbContext context,
        INotificationService notificationService,
        IEmailService emailService,
        IActivityLogService activityLogService,
        ILogger<CourseEditApprovalService> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _emailService = emailService;
        _activityLogService = activityLogService;
        _logger = logger;
    }

    public async Task<EditResultDto> RequestEditAsync(Guid courseId, Guid instructorId, EditRequestType targetType, EditOperation operation, string? propertyName, Guid? targetEntityId, string? payloadJson, CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses
            .Include(c => c.Creator)
            .FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken)
            ?? throw new KeyNotFoundException("Course not found.");

        if (course.CreatedBy != instructorId)
            throw new UnauthorizedAccessException("You do not own this course.");

        var requiresApproval = EditPolicyHelper.RequiresApproval(course, targetType, operation, propertyName);

        if (!requiresApproval)
        {
            await ApplyDirectEditAsync(course, propertyName, payloadJson, cancellationToken);

            _context.CourseLogs.Add(new CourseLog
            {
                CourseId = courseId,
                UserId = instructorId,
                Action = CourseLogAction.Updated,
                Details = $"Direct edit: {propertyName ?? targetType.ToString()}",
                Timestamp = DateTime.UtcNow
            });

            await _activityLogService.LogActivityAsync(
                userId: instructorId,
                action: "Course.Edited",
                description: $"Modified {targetType} in course '{course.Title}'",
                ipAddress: "system",
                cancellationToken: cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return new EditResultDto
            {
                AppliedImmediately = true,
                Message = "Edit applied successfully",
                ProcessedAt = DateTime.UtcNow
            };
        }

        var request = new CourseEditRequest
        {
            CourseId = course.Id,
            RequestedBy = instructorId,
            RequestType = targetType,
            TargetSectionId = targetType == EditRequestType.Section ? targetEntityId : null,
            TargetItemId = targetType == EditRequestType.SectionItem ? targetEntityId : null,
            Operation = operation,
            JsonPayload = payloadJson,
            Status = EditRequestStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow + EditPolicyHelper.GetExpirationPeriod(targetType, operation),
            IsEmergency = EditPolicyHelper.AssessRisk(targetType, operation) >= EditRiskLevel.High
        };

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            _context.CourseEditRequests.Add(request);

            _context.CourseLogs.Add(new CourseLog
            {
                CourseId = courseId,
                UserId = instructorId,
                Action = CourseLogAction.Updated,
                Details = $"Edit request {targetType} - Status: Pending",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);

            await _activityLogService.LogActivityAsync(
                userId: instructorId,
                action: "CourseEdit.Requested",
                description: $"Submitted edit request #{request.Id} for course '{course.Title}'",
                ipAddress: "system",
                cancellationToken: cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        _ = NotifyAdminsAsync(course, request.Id, request.IsEmergency);

        return new EditResultDto
        {
            AppliedImmediately = false,
            RequestId = request.Id,
            Status = EditRequestStatus.Pending,
            Message = "Edit request submitted for approval",
            ProcessedAt = DateTime.UtcNow
        };
    }

    public async Task<PagedList<EditRequestSummaryDto>> GetPendingRequestsAsync(EditRequestFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _context.CourseEditRequests
            .AsNoTracking()
            .Include(r => r.Course)
            .Include(r => r.RequestedByUser)
            .AsQueryable();

        if (filter.Status.HasValue)
            query = query.Where(r => r.Status == filter.Status.Value);
        else
            query = query.Where(r => r.Status == EditRequestStatus.Pending);

        if (filter.CourseId.HasValue)
            query = query.Where(r => r.CourseId == filter.CourseId.Value);

        if (filter.InstructorId.HasValue)
            query = query.Where(r => r.RequestedBy == filter.InstructorId.Value);

        if (filter.RequestType.HasValue)
            query = query.Where(r => r.RequestType == filter.RequestType.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.IsEmergency)
            .ThenByDescending(r => r.RequestedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(r => new EditRequestSummaryDto
            {
                Id = r.Id,
                CourseTitle = r.Course.Title,
                InstructorName = r.RequestedByUser.FirstName + " " + r.RequestedByUser.LastName,
                RequestType = r.RequestType,
                Operation = r.Operation,
                Status = r.Status,
                RequestedAt = r.RequestedAt,
                TimeUntilExpiry = r.ExpiresAt.HasValue
                    ? r.ExpiresAt.Value - DateTime.UtcNow
                    : TimeSpan.Zero,
                IsEmergency = r.IsEmergency
            })
            .ToListAsync(cancellationToken);

        return new PagedList<EditRequestSummaryDto>(items, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<EditRequestDetailDto> GetRequestDetailsAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var request = await _context.CourseEditRequests
            .AsNoTracking()
            .Include(r => r.Course)
            .Include(r => r.RequestedByUser)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken)
            ?? throw new KeyNotFoundException("Edit request not found.");

        return new EditRequestDetailDto
        {
            RequestId = request.Id,
            CourseId = request.CourseId,
            CourseTitle = request.Course.Title,
            InstructorId = request.RequestedBy,
            InstructorName = $"{request.RequestedByUser.FirstName} {request.RequestedByUser.LastName}",
            TargetType = request.RequestType,
            Operation = request.Operation,
            RequestedAt = request.RequestedAt,
            ExpiresAt = request.ExpiresAt,
            RiskLevel = EditPolicyHelper.AssessRisk(request.RequestType, request.Operation),
            Changes = await GenerateChangesListAsync(request, cancellationToken)
        };
    }

    public async Task<EditResultDto> ReviewRequestAsync(Guid requestId, Guid adminId, bool approve, string? notes, CancellationToken cancellationToken = default)
    {
        var request = await _context.CourseEditRequests
            .Include(r => r.Course)
            .Include(r => r.RequestedByUser)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.Status == EditRequestStatus.Pending, cancellationToken)
            ?? throw new KeyNotFoundException("Edit request not found or already processed.");

        var course = request.Course;
        var instructor = request.RequestedByUser;

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            if (approve)
            {
                await ApplyEditFromRequestAsync(request, cancellationToken);

                request.Status = EditRequestStatus.Approved;
                request.ReviewedAt = DateTime.UtcNow;
                request.ReviewedBy = adminId;
                request.AdminNotes = notes;

                course.Version++;
                course.LastContentUpdateAt = DateTime.UtcNow;
                course.UpdatedAt = DateTime.UtcNow;

                _context.CourseLogs.Add(new CourseLog
                {
                    CourseId = course.Id,
                    UserId = adminId,
                    Action = CourseLogAction.ContentModified,
                    Details = $"Edit approved by Admin{(string.IsNullOrEmpty(notes) ? "" : $": {notes}")}",
                    Timestamp = DateTime.UtcNow
                });
            }
            else
            {
                request.Status = EditRequestStatus.Rejected;
                request.ReviewedAt = DateTime.UtcNow;
                request.ReviewedBy = adminId;
                request.AdminNotes = notes;

                _context.CourseLogs.Add(new CourseLog
                {
                    CourseId = course.Id,
                    UserId = adminId,
                    Action = CourseLogAction.Updated,
                    Details = $"Edit rejected: {notes ?? "No reason provided"}",
                    Timestamp = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await _activityLogService.LogActivityAsync(
                userId: adminId,
                action: approve ? "CourseEdit.Approved" : "CourseEdit.Rejected",
                description: $"{(approve ? "Approved" : "Rejected")} edit request #{requestId} for course '{course.Title}'",
                ipAddress: "system",
                cancellationToken: cancellationToken);

            _ = NotifyInstructorAsync(instructor.Id, course.Title, approve, notes);

            _ = (approve
                ? _emailService.SendInstructorRequestApprovedAsync(instructor.Email!, instructor.FullName, notes)
                : _emailService.SendInstructorRequestRejectedAsync(instructor.Email!, instructor.FullName, notes, null));

            return new EditResultDto
            {
                AppliedImmediately = false,
                RequestId = requestId,
                Status = approve ? EditRequestStatus.Approved : EditRequestStatus.Rejected,
                Message = approve ? "Edit approved and applied" : "Edit rejected",
                ProcessedAt = DateTime.UtcNow
            };
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> CancelRequestAsync(Guid requestId, Guid instructorId, CancellationToken cancellationToken = default)
    {
        var request = await _context.CourseEditRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.RequestedBy == instructorId && r.Status == EditRequestStatus.Pending, cancellationToken);

        if (request is null)
            return false;

        request.Status = EditRequestStatus.Cancelled;
        request.AdminNotes = "Cancelled by instructor";

        _context.CourseLogs.Add(new CourseLog
        {
            CourseId = request.CourseId,
            UserId = instructorId,
            Action = CourseLogAction.Updated,
            Details = "Edit request cancelled by instructor",
            Timestamp = DateTime.UtcNow
        });

        await _activityLogService.LogActivityAsync(
            userId: instructorId,
            action: "CourseEdit.Cancelled",
            description: $"Cancelled edit request #{requestId}",
            ipAddress: "system",
            cancellationToken: cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> CleanupExpiredRequestsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expired = await _context.CourseEditRequests
            .Where(r => r.Status == EditRequestStatus.Pending && r.ExpiresAt < now)
            .ToListAsync(cancellationToken);

        if (expired.Count == 0) return 0;

        foreach (var request in expired)
        {
            request.Status = EditRequestStatus.Expired;
            request.AdminNotes = "Auto-expired: No review within allowed period";

            _context.CourseLogs.Add(new CourseLog
            {
                CourseId = request.CourseId,
                UserId = request.RequestedBy,
                Action = CourseLogAction.Updated,
                Details = "Edit request auto-expired",
                Timestamp = DateTime.UtcNow
            });
        }

        var count = await _context.SaveChangesAsync(cancellationToken);
        return count;
    }

    private Task ApplyDirectEditAsync(Domain.Entities.Course course, string? propertyName, string? payload, CancellationToken cancellationToken)
    {
        if (propertyName == nameof(Domain.Entities.Course.Description) && !string.IsNullOrEmpty(payload))
        {
            course.Description = payload;
            course.UpdatedAt = DateTime.UtcNow;
        }
        else if (propertyName == nameof(Domain.Entities.Course.Slug) && !string.IsNullOrEmpty(payload))
        {
            course.Slug = payload;
            course.UpdatedAt = DateTime.UtcNow;
        }

        return Task.CompletedTask;
    }

    private async Task ApplyEditFromRequestAsync(CourseEditRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.JsonPayload) && request.Operation != EditOperation.Delete)
            return;

        switch (request.RequestType)
        {
            case EditRequestType.Section when request.TargetSectionId.HasValue:
                await ApplySectionEditAsync(request, cancellationToken);
                break;

            case EditRequestType.SectionItem when request.TargetItemId.HasValue:
                await ApplySectionItemEditAsync(request, cancellationToken);
                break;

            case EditRequestType.CourseProperty:
                await ApplyCoursePropertyEditAsync(request, cancellationToken);
                break;
        }
    }

    private async Task ApplySectionEditAsync(CourseEditRequest request, CancellationToken cancellationToken)
    {
        var section = await _context.Sections.FindAsync(new object[] { request.TargetSectionId! }, cancellationToken);
        if (section is null) return;

        if (request.Operation == EditOperation.Delete)
        {
            _context.Sections.Remove(section);
        }
        else if (request.Operation == EditOperation.Update && !string.IsNullOrEmpty(request.JsonPayload))
        {
            var payload = JsonSerializer.Deserialize<SectionEditPayload>(request.JsonPayload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (payload is not null)
            {
                if (payload.Title is not null) section.Title = payload.Title;
                if (payload.Description is not null) section.Description = payload.Description;
                if (payload.Position.HasValue) section.Position = payload.Position.Value;
            }
        }
    }

    private async Task ApplySectionItemEditAsync(CourseEditRequest request, CancellationToken cancellationToken)
    {
        var item = await _context.SectionItems.FindAsync(new object[] { request.TargetItemId! }, cancellationToken);
        if (item is null) return;

        if (request.Operation == EditOperation.Delete)
        {
            _context.SectionItems.Remove(item);
        }
        else if (request.Operation == EditOperation.Update && !string.IsNullOrEmpty(request.JsonPayload))
        {
            var payload = JsonSerializer.Deserialize<SectionItemEditPayload>(request.JsonPayload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (payload is not null)
            {
                if (payload.IsPreviewAllowed.HasValue) item.IsPreviewAllowed = payload.IsPreviewAllowed.Value;
                if (payload.IsMandatory.HasValue) item.IsMandatory = payload.IsMandatory.Value;
                if (payload.Position.HasValue) item.Position = payload.Position.Value;
            }
        }
    }

    private async Task ApplyCoursePropertyEditAsync(CourseEditRequest request, CancellationToken cancellationToken)
    {
        var course = request.Course ?? await _context.Courses.FindAsync(new object[] { request.CourseId }, cancellationToken);
        if (course is null || string.IsNullOrEmpty(request.JsonPayload)) return;

        var payload = JsonSerializer.Deserialize<CoursePropertyEditPayload>(request.JsonPayload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (payload is not null)
        {
            if (payload.Price.HasValue) course.Price = payload.Price.Value;
            if (payload.Title is not null) course.Title = payload.Title;
            if (payload.Description is not null) course.Description = payload.Description;
            if (payload.CategoryId.HasValue) course.CategoryId = payload.CategoryId.Value;
            course.UpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task<List<FieldChangeDto>> GenerateChangesListAsync(CourseEditRequest request, CancellationToken cancellationToken)
    {
        var changes = new List<FieldChangeDto>();

        if (request.Operation == EditOperation.Delete)
        {
            changes.Add(new FieldChangeDto
            {
                FieldName = "_deletion",
                FieldLabel = request.RequestType switch
                {
                    EditRequestType.Section => "Section",
                    EditRequestType.SectionItem => "Section Item",
                    EditRequestType.CourseProperty => "Property",
                    _ => "Element"
                },
                OldValue = "Exists",
                NewValue = "Will be deleted",
                ChangeType = ChangeType.Deleted
            });
            return changes;
        }

        if (string.IsNullOrEmpty(request.JsonPayload)) return changes;

        try
        {
            if (request.RequestType == EditRequestType.Section && request.TargetSectionId.HasValue)
            {
                var oldSection = await _context.Sections.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == request.TargetSectionId.Value, cancellationToken);
                var payload = JsonSerializer.Deserialize<SectionEditPayload>(request.JsonPayload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (oldSection is not null && payload is not null)
                {
                    if (payload.Title is not null && oldSection.Title != payload.Title)
                        changes.Add(CreateFieldChange("Title", "Section Title", oldSection.Title, payload.Title));
                    if (payload.Description is not null && oldSection.Description != payload.Description)
                        changes.Add(CreateFieldChange("Description", "Description", oldSection.Description, payload.Description));
                }
            }
            else if (request.RequestType == EditRequestType.SectionItem && request.TargetItemId.HasValue)
            {
                var oldItem = await _context.SectionItems.AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Id == request.TargetItemId.Value, cancellationToken);
                var payload = JsonSerializer.Deserialize<SectionItemEditPayload>(request.JsonPayload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (oldItem is not null && payload is not null)
                {
                    if (payload.IsPreviewAllowed.HasValue && oldItem.IsPreviewAllowed != payload.IsPreviewAllowed.Value)
                        changes.Add(CreateFieldChange("IsPreviewAllowed", "Free Preview", oldItem.IsPreviewAllowed, payload.IsPreviewAllowed.Value));
                    if (payload.IsMandatory.HasValue && oldItem.IsMandatory != payload.IsMandatory.Value)
                        changes.Add(CreateFieldChange("IsMandatory", "Mandatory", oldItem.IsMandatory, payload.IsMandatory.Value));
                }
            }
            else if (request.RequestType == EditRequestType.CourseProperty)
            {
                var oldCourse = await _context.Courses.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);
                var payload = JsonSerializer.Deserialize<CoursePropertyEditPayload>(request.JsonPayload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (oldCourse is not null && payload is not null)
                {
                    if (payload.Title is not null && oldCourse.Title != payload.Title)
                        changes.Add(CreateFieldChange("Title", "Course Title", oldCourse.Title, payload.Title));
                    if (payload.Price.HasValue && oldCourse.Price != payload.Price.Value)
                        changes.Add(CreateFieldChange("Price", "Price", oldCourse.Price, payload.Price.Value));
                    if (payload.Description is not null && oldCourse.Description != payload.Description)
                        changes.Add(CreateFieldChange("Description", "Description", oldCourse.Description, payload.Description));
                    if (payload.CategoryId.HasValue && oldCourse.CategoryId != payload.CategoryId.Value)
                        changes.Add(CreateFieldChange("CategoryId", "Category", oldCourse.CategoryId, payload.CategoryId.Value));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating changes list for request {RequestId}", request.Id);
        }

        return changes;
    }

    private static FieldChangeDto CreateFieldChange(string fieldName, string fieldLabel, object? oldValue, object? newValue)
    {
        return new FieldChangeDto
        {
            FieldName = fieldName,
            FieldLabel = fieldLabel,
            OldValue = oldValue,
            NewValue = newValue,
            ChangeType = ChangeType.Modified
        };
    }

    private async Task NotifyAdminsAsync(Domain.Entities.Course course, Guid requestId, bool isEmergency)
    {
        try
        {
            var adminIds = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.Role.Name == "Admin")
                .Select(ur => ur.UserId)
                .ToListAsync();

            foreach (var adminId in adminIds)
            {
                await _notificationService.CreateAndSendNotificationAsync(
                    userId: adminId,
                    title: isEmergency ? "Urgent Edit Request" : "New Edit Request",
                    message: $"Instructor requests edit on course: {course.Title}",
                    type: NotificationType.System,
                    linkUrl: $"/admin/edit-requests/{requestId}",
                    icon: isEmergency ? "alert-triangle" : "alert-circle");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify admins for request {RequestId}", requestId);
        }
    }

    private async Task NotifyInstructorAsync(Guid instructorId, string courseTitle, bool approved, string? notes)
    {
        try
        {
            await _notificationService.CreateAndSendNotificationAsync(
                userId: instructorId,
                title: approved ? "Edit Approved" : "Edit Rejected",
                message: approved
                    ? $"Your edit on \"{courseTitle}\" has been published"
                    : $"Your edit on \"{courseTitle}\" was rejected.\nReason: {notes ?? "Not specified"}",
                type: NotificationType.System,
                linkUrl: $"/courses/{courseTitle}",
                icon: approved ? "check-circle" : "x-circle");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify instructor {InstructorId}", instructorId);
        }
    }
}

internal sealed record SectionEditPayload
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public int? Position { get; init; }
}

internal sealed record SectionItemEditPayload
{
    public bool? IsPreviewAllowed { get; init; }
    public bool? IsMandatory { get; init; }
    public int? Position { get; init; }
}

internal sealed record CoursePropertyEditPayload
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public decimal? Price { get; init; }
    public Guid? CategoryId { get; init; }
}

internal static class EditPolicyHelper
{
    private static readonly HashSet<string> SensitiveProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(Domain.Entities.Course.Price),
        nameof(Domain.Entities.Course.Title),
        nameof(Domain.Entities.Course.CategoryId),
        nameof(Domain.Entities.Course.Level),
        nameof(Domain.Entities.Course.Language)
    };

    internal static bool RequiresApproval(Domain.Entities.Course course, EditRequestType targetType, EditOperation operation, string? propertyName)
    {
        if (course.Status != CourseStatus.Published)
            return false;

        if (operation == EditOperation.Delete)
            return true;

        if (targetType == EditRequestType.SectionItem || targetType == EditRequestType.Section)
            return true;

        if (!string.IsNullOrEmpty(propertyName) && SensitiveProperties.Contains(propertyName))
            return true;

        return false;
    }

    internal static EditRiskLevel AssessRisk(EditRequestType type, EditOperation operation)
    {
        if (operation == EditOperation.Delete)
            return EditRiskLevel.High;

        if (type == EditRequestType.SectionItem)
            return EditRiskLevel.High;

        if (type == EditRequestType.Section)
            return EditRiskLevel.Medium;

        return EditRiskLevel.Low;
    }

    internal static TimeSpan GetExpirationPeriod(EditRequestType type, EditOperation operation)
    {
        return operation == EditOperation.Delete ? TimeSpan.FromDays(3) : TimeSpan.FromDays(7);
    }
}
