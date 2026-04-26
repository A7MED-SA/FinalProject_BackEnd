using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using backend_project.Data;
using backend_project.Models;
using backend_project.DTOs.EditRequest;
using backend_project.Helpers;
using backend_project.Services.Interfaces;
using backend_project.Services.Notifications;

namespace backend_project.Services;

/// <summary>
/// Core service implementing the edit approval workflow.
/// Integrates with NotificationService, EmailService, ActivityLogService, and CourseLog.
/// </summary>
public class CourseEditApprovalService : ICourseEditApprovalService
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

    public async Task<EditResultDto> RequestEditAsync(
        Guid courseId,
        Guid instructorId,
        EditContext context,
        string? payloadJson)
    {
        var course = await _context.Courses
            .Include(c => c.Creator)
            .FirstOrDefaultAsync(c => c.Id == courseId)
            ?? throw new KeyNotFoundException($"Course {courseId} not found");

        if (course.CreatedBy != instructorId)
            throw new UnauthorizedAccessException("You do not own this course");

        bool requiresApproval = EditPolicyHelper.RequiresApproval(course, context);

        if (!requiresApproval)
        {
            // Apply edit directly
            var result = await ApplyEditDirectlyAsync(course, context, payloadJson);

            await LogToCourseLogAsync(
                courseId, instructorId,
                CourseLogAction.Updated,
                $"Direct edit: {context.PropertyName ?? context.TargetType.ToString()}");

            await _activityLogService.LogActivityAsync(
                userId: instructorId,
                action: "Course.Edited",
                description: $"Modified {context.TargetType} in course '{course.Title}'",
                ipAddress: "system");

            _logger.LogInformation(
                "Direct edit applied to course {CourseId} by instructor {InstructorId}",
                courseId, instructorId);

            return new EditResultDto
            {
                AppliedImmediately = true,
                Data = result,
                Message = "Edit applied successfully"
            };
        }

        // Create a pending review request
        var request = new CourseEditRequest
        {
            CourseId = course.Id,
            RequestedBy = instructorId,
            RequestType = context.TargetType,
            TargetSectionId = context.TargetType == EditRequestType.Section ? context.TargetEntityId : null,
            TargetItemId = context.TargetType == EditRequestType.SectionItem ? context.TargetEntityId : null,
            Operation = context.Operation,
            JsonPayload = payloadJson,
            Status = EditRequestStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow + EditPolicyHelper.GetExpirationPeriod(context.TargetType, context.Operation),
            IsEmergency = EditPolicyHelper.AssessRisk(context) >= EditRiskLevel.High
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.CourseEditRequests.Add(request);
            await _context.SaveChangesAsync();

            await LogToCourseLogAsync(
                courseId, instructorId,
                CourseLogAction.EditRequestSubmitted,
                $"Edit request {context.TargetType} - Status: Pending");

            await _activityLogService.LogActivityAsync(
                userId: instructorId,
                action: "CourseEdit.Requested",
                description: $"Submitted edit request #{request.Id} for course '{course.Title}'",
                ipAddress: "system");

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        // Fire-and-forget: notify admins (don't block the response)
        _ = NotifyAdminsAsync(course, request.Id, request.IsEmergency)
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                    _logger.LogError(t.Exception, "Failed to notify admins for request {RequestId}", request.Id);
            });

        _logger.LogInformation(
            "Edit request {RequestId} created for course {CourseId} by instructor {InstructorId}",
            request.Id, courseId, instructorId);

        return new EditResultDto
        {
            AppliedImmediately = false,
            RequestId = request.Id,
            Status = EditRequestStatus.Pending,
            Message = "Edit request submitted for approval",
            ProcessedAt = DateTime.UtcNow
        };
    }

    public async Task<PagedResult<EditRequestSummaryDto>> GetPendingRequestsAsync(EditRequestFilterDto filter)
    {
        var query = _context.CourseEditRequests
            .AsNoTracking()
            .Include(r => r.Course)
            .Include(r => r.RequestedByUser)
            .AsQueryable();

        // Default to pending if no status filter
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

        var totalCount = await query.CountAsync();

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
            .ToListAsync();

        return new PagedResult<EditRequestSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<EditRequestDetailDto> GetRequestDetailsAsync(Guid requestId)
    {
        var request = await _context.CourseEditRequests
            .AsNoTracking()
            .Include(r => r.Course)
            .Include(r => r.RequestedByUser)
            .FirstOrDefaultAsync(r => r.Id == requestId)
            ?? throw new KeyNotFoundException($"Edit request {requestId} not found");

        var dto = new EditRequestDetailDto
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
            RiskLevel = EditPolicyHelper.AssessRisk(new EditContext
            {
                TargetType = request.RequestType,
                Operation = request.Operation
            })
        };

        dto.Changes = await GenerateChangesListAsync(request);
        dto.StudentImpact = await CalculateStudentImpactAsync(request);

        return dto;
    }

    public async Task<EditResultDto> ReviewRequestAsync(
        Guid requestId,
        Guid adminId,
        bool approve,
        string? notes)
    {
        var request = await _context.CourseEditRequests
            .Include(r => r.Course)
            .Include(r => r.RequestedByUser)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.Status == EditRequestStatus.Pending)
            ?? throw new KeyNotFoundException("Edit request not found or already processed");

        var course = request.Course;
        var instructor = request.RequestedByUser;

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (approve)
            {
                await ApplyEditFromRequestAsync(request);

                request.Status = EditRequestStatus.Approved;
                request.ReviewedAt = DateTime.UtcNow;
                request.ReviewedBy = adminId;
                request.AdminNotes = notes;

                course.Version++;
                course.LastContentUpdateAt = DateTime.UtcNow;
                course.UpdatedAt = DateTime.UtcNow;

                await LogToCourseLogAsync(
                    course.Id, adminId,
                    CourseLogAction.EditRequestApproved,
                    $"Edit approved by Admin{(string.IsNullOrEmpty(notes) ? "" : $": {notes}")}");
            }
            else
            {
                request.Status = EditRequestStatus.Rejected;
                request.ReviewedAt = DateTime.UtcNow;
                request.ReviewedBy = adminId;
                request.AdminNotes = notes;

                await LogToCourseLogAsync(
                    course.Id, adminId,
                    CourseLogAction.EditRequestRejected,
                    $"Edit rejected: {notes ?? "No reason provided"}");
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _activityLogService.LogActivityAsync(
                userId: adminId,
                action: approve ? "CourseEdit.Approved" : "CourseEdit.Rejected",
                description: $"{(approve ? "Approved" : "Rejected")} edit request #{requestId} for course '{course.Title}'",
                ipAddress: "system");

            // Fire-and-forget: notify instructor
            _ = NotifyInstructorAsync(instructor.Id, course.Title, approve, notes)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        _logger.LogError(t.Exception, "Failed to notify instructor {InstructorId}", instructor.Id);
                });

            // Fire-and-forget: email instructor
            _ = (approve
                ? _emailService.SendTeacherRequestApprovedAsync(instructor.Email!, instructor.FullName, notes)
                : _emailService.SendTeacherRequestRejectedAsync(instructor.Email!, instructor.FullName, notes, null))
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        _logger.LogError(t.Exception, "Failed to send email to instructor {Email}", instructor.Email);
                });

            _logger.LogInformation(
                "Edit request {RequestId} {Action} by admin {AdminId}",
                requestId, approve ? "approved" : "rejected", adminId);

            return new EditResultDto
            {
                AppliedImmediately = false,
                RequestId = requestId,
                Status = approve ? EditRequestStatus.Approved : EditRequestStatus.Rejected,
                Message = approve ? "Edit approved and applied" : "Edit rejected",
                ProcessedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to process edit request {RequestId}", requestId);
            throw;
        }
    }

    public async Task<bool> CancelRequestAsync(Guid requestId, Guid instructorId)
    {
        var request = await _context.CourseEditRequests
            .FirstOrDefaultAsync(r => r.Id == requestId &&
                                     r.RequestedBy == instructorId &&
                                     r.Status == EditRequestStatus.Pending);

        if (request == null) return false;

        request.Status = EditRequestStatus.Cancelled;
        request.AdminNotes = "Cancelled by instructor";

        await LogToCourseLogAsync(
            request.CourseId, instructorId,
            CourseLogAction.EditRequestCancelled,
            "Edit request cancelled by instructor");

        await _activityLogService.LogActivityAsync(
            userId: instructorId,
            action: "CourseEdit.Cancelled",
            description: $"Cancelled edit request #{requestId}",
            ipAddress: "system");

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Edit request {RequestId} cancelled by instructor {InstructorId}",
            requestId, instructorId);

        return true;
    }

    public async Task<int> CleanupExpiredRequestsAsync()
    {
        var now = DateTime.UtcNow;
        var expired = await _context.CourseEditRequests
            .Where(r => r.Status == EditRequestStatus.Pending && r.ExpiresAt < now)
            .ToListAsync();

        if (!expired.Any()) return 0;

        foreach (var request in expired)
        {
            request.Status = EditRequestStatus.Expired;
            request.AdminNotes = "Auto-expired: No review within allowed period";

            _context.CourseLogs.Add(new CourseLog
            {
                CourseId = request.CourseId,
                UserId = request.RequestedBy,
                Action = CourseLogAction.EditRequestRejected,
                Details = "Edit request auto-expired",
                Timestamp = DateTime.UtcNow
            });
        }

        var count = await _context.SaveChangesAsync();
        _logger.LogInformation("Cleaned up {Count} expired edit requests", expired.Count);

        return expired.Count;
    }

    #region Private Helper Methods

    private async Task<object?> ApplyEditDirectlyAsync(Course course, EditContext context, string? payload)
    {
        if (context.PropertyName == nameof(Course.Description) && !string.IsNullOrEmpty(payload))
        {
            course.Description = payload;
            course.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        else if (context.PropertyName == nameof(Course.Slug) && !string.IsNullOrEmpty(payload))
        {
            course.Slug = payload;
            course.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return course;
    }

    private async Task ApplyEditFromRequestAsync(CourseEditRequest request)
    {
        if (string.IsNullOrEmpty(request.JsonPayload) && request.Operation != EditOperation.Delete)
            return;

        switch (request.RequestType)
        {
            case EditRequestType.Section when request.TargetSectionId.HasValue:
                await ApplySectionEditAsync(request);
                break;

            case EditRequestType.SectionItem when request.TargetItemId.HasValue:
                await ApplySectionItemEditAsync(request);
                break;

            case EditRequestType.CourseProperty:
                await ApplyCoursePropertyEditAsync(request);
                break;
        }
    }

    private async Task ApplySectionEditAsync(CourseEditRequest request)
    {
        var section = await _context.Sections.FindAsync(request.TargetSectionId);
        if (section == null) return;

        if (request.Operation == EditOperation.Delete)
        {
            _context.Sections.Remove(section);
        }
        else if (request.Operation == EditOperation.Update && !string.IsNullOrEmpty(request.JsonPayload))
        {
            var payload = JsonSerializer.Deserialize<SectionEditPayload>(request.JsonPayload,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (payload != null)
            {
                if (payload.Title != null) section.Title = payload.Title;
                if (payload.Description != null) section.Description = payload.Description;
                if (payload.IsLocked.HasValue) section.IsLocked = payload.IsLocked.Value;
                if (payload.Position.HasValue) section.Position = payload.Position.Value;
            }
        }
    }

    private async Task ApplySectionItemEditAsync(CourseEditRequest request)
    {
        var item = await _context.SectionItems.FindAsync(request.TargetItemId);
        if (item == null) return;

        if (request.Operation == EditOperation.Delete)
        {
            _context.SectionItems.Remove(item);
        }
        else if (request.Operation == EditOperation.Update && !string.IsNullOrEmpty(request.JsonPayload))
        {
            var payload = JsonSerializer.Deserialize<SectionItemEditPayload>(request.JsonPayload,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (payload != null)
            {
                if (payload.IsPreviewAllowed.HasValue) item.IsPreviewAllowed = payload.IsPreviewAllowed.Value;
                if (payload.IsMandatory.HasValue) item.IsMandatory = payload.IsMandatory.Value;
                if (payload.Position.HasValue) item.Position = payload.Position.Value;
            }
        }
    }

    private async Task ApplyCoursePropertyEditAsync(CourseEditRequest request)
    {
        var course = request.Course ?? await _context.Courses.FindAsync(request.CourseId);
        if (course == null || string.IsNullOrEmpty(request.JsonPayload)) return;

        var payload = JsonSerializer.Deserialize<CoursePropertyEditPayload>(request.JsonPayload,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (payload != null)
        {
            if (payload.Price.HasValue) course.Price = payload.Price.Value;
            if (payload.Title != null) course.Title = payload.Title;
            if (payload.Description != null) course.Description = payload.Description;
            if (payload.CategoryId.HasValue) course.CategoryId = payload.CategoryId.Value;
            course.UpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task<List<FieldChangeDto>> GenerateChangesListAsync(CourseEditRequest request)
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
                    .FirstOrDefaultAsync(s => s.Id == request.TargetSectionId.Value);
                var sectionPayload = JsonSerializer.Deserialize<SectionEditPayload>(request.JsonPayload,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (oldSection != null && sectionPayload != null)
                {
                    if (sectionPayload.Title != null && oldSection.Title != sectionPayload.Title)
                        changes.Add(CreateFieldChange("Title", "Section Title", oldSection.Title, sectionPayload.Title));
                    if (sectionPayload.Description != null && oldSection.Description != sectionPayload.Description)
                        changes.Add(CreateFieldChange("Description", "Description", oldSection.Description, sectionPayload.Description));
                    if (sectionPayload.IsLocked.HasValue && oldSection.IsLocked != sectionPayload.IsLocked.Value)
                        changes.Add(CreateFieldChange("IsLocked", "Locked", oldSection.IsLocked, sectionPayload.IsLocked.Value));
                }
            }
            else if (request.RequestType == EditRequestType.SectionItem && request.TargetItemId.HasValue)
            {
                var oldItem = await _context.SectionItems.AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Id == request.TargetItemId.Value);
                var itemPayload = JsonSerializer.Deserialize<SectionItemEditPayload>(request.JsonPayload,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (oldItem != null && itemPayload != null)
                {
                    if (itemPayload.IsPreviewAllowed.HasValue && oldItem.IsPreviewAllowed != itemPayload.IsPreviewAllowed.Value)
                        changes.Add(CreateFieldChange("IsPreviewAllowed", "Free Preview", oldItem.IsPreviewAllowed, itemPayload.IsPreviewAllowed.Value));
                    if (itemPayload.IsMandatory.HasValue && oldItem.IsMandatory != itemPayload.IsMandatory.Value)
                        changes.Add(CreateFieldChange("IsMandatory", "Mandatory", oldItem.IsMandatory, itemPayload.IsMandatory.Value));
                }
            }
            else if (request.RequestType == EditRequestType.CourseProperty)
            {
                var oldCourse = await _context.Courses.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == request.CourseId);
                var propPayload = JsonSerializer.Deserialize<CoursePropertyEditPayload>(request.JsonPayload,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (oldCourse != null && propPayload != null)
                {
                    if (propPayload.Title != null && oldCourse.Title != propPayload.Title)
                        changes.Add(CreateFieldChange("Title", "Course Title", oldCourse.Title, propPayload.Title));
                    if (propPayload.Price.HasValue && oldCourse.Price != propPayload.Price.Value)
                        changes.Add(CreateFieldChange("Price", "Price", oldCourse.Price, propPayload.Price.Value));
                    if (propPayload.Description != null && oldCourse.Description != propPayload.Description)
                        changes.Add(CreateFieldChange("Description", "Description", oldCourse.Description, propPayload.Description));
                    if (propPayload.CategoryId.HasValue && oldCourse.CategoryId != propPayload.CategoryId.Value)
                        changes.Add(CreateFieldChange("CategoryId", "Category", oldCourse.CategoryId, propPayload.CategoryId.Value));
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

    private async Task<StudentImpactDto?> CalculateStudentImpactAsync(CourseEditRequest request)
    {
        if (request.Operation != EditOperation.Delete) return null;

        var enrollmentCount = await _context.Enrollments
            .CountAsync(e => e.CourseId == request.CourseId &&
                            e.Status == EnrollmentStatus.InProgress);

        return new StudentImpactDto
        {
            AffectedEnrollments = enrollmentCount,
            WillLoseProgress = request.RequestType == EditRequestType.SectionItem,
            ImpactDescriptions = new List<string>
            {
                $"Will affect {enrollmentCount} active student(s)",
                request.RequestType == EditRequestType.Section
                    ? "Students may lose progress in this section"
                    : "Content will be removed from the course"
            }
        };
    }

    private async Task LogToCourseLogAsync(Guid courseId, Guid userId, CourseLogAction action, string details)
    {
        _context.CourseLogs.Add(new CourseLog
        {
            CourseId = courseId,
            UserId = userId,
            Action = action,
            Details = details,
            Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }

    private async Task NotifyAdminsAsync(Course course, Guid requestId, bool isEmergency)
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
                title: isEmergency ? "🚨 Urgent Edit Request" : "🔔 New Edit Request",
                message: $"Instructor requests edit on course: {course.Title}",
                type: NotificationType.System,
                linkUrl: $"/admin/edit-requests/{requestId}",
                icon: isEmergency ? "alert-triangle" : "alert-circle"
            );
        }
    }

    private async Task NotifyInstructorAsync(Guid instructorId, string courseTitle, bool approved, string? notes)
    {
        await _notificationService.CreateAndSendNotificationAsync(
            userId: instructorId,
            title: approved ? "✅ Edit Approved" : "❌ Edit Rejected",
            message: approved
                ? $"Your edit on \"{courseTitle}\" has been published"
                : $"Your edit on \"{courseTitle}\" was rejected.\nReason: {notes ?? "Not specified"}",
            type: NotificationType.System,
            linkUrl: $"/courses/{courseTitle}",
            icon: approved ? "check-circle" : "x-circle"
        );
    }

    #endregion
}

// JSON Payload classes for deserialization
public class SectionEditPayload
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool? IsLocked { get; set; }
    public int? Position { get; set; }
}

public class SectionItemEditPayload
{
    public bool? IsPreviewAllowed { get; set; }
    public bool? IsMandatory { get; set; }
    public int? Position { get; set; }
}

public class CoursePropertyEditPayload
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public Guid? CategoryId { get; set; }
    public CourseLevel? Level { get; set; }
}
