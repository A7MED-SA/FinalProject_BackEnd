using backend_project.Models;

namespace backend_project.DTOs.EditRequest;

/// <summary>
/// Result returned after submitting or reviewing an edit request
/// </summary>
public class EditResultDto
{
    public bool AppliedImmediately { get; set; }
    public Guid? RequestId { get; set; }
    public EditRequestStatus? Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Summary view of an edit request (used in listing)
/// </summary>
public class EditRequestSummaryDto
{
    public Guid Id { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public EditRequestType RequestType { get; set; }
    public EditOperation Operation { get; set; }
    public EditRequestStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public TimeSpan TimeUntilExpiry { get; set; }
    public EditRiskLevel RiskLevel { get; set; }
    public bool IsEmergency { get; set; }
}

/// <summary>
/// Detailed view of an edit request with diff and impact analysis
/// </summary>
public class EditRequestDetailDto
{
    public Guid RequestId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public Guid InstructorId { get; set; }
    public EditRequestType TargetType { get; set; }
    public EditOperation Operation { get; set; }
    public List<FieldChangeDto> Changes { get; set; } = new();
    public DateTime RequestedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public EditRiskLevel RiskLevel { get; set; }
    public StudentImpactDto? StudentImpact { get; set; }
}

/// <summary>
/// Represents a single field change (old vs new)
/// </summary>
public class FieldChangeDto
{
    public string FieldName { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
    public ChangeType ChangeType { get; set; }
}

public enum ChangeType { Added, Modified, Deleted }

/// <summary>
/// Student impact analysis for a destructive edit
/// </summary>
public class StudentImpactDto
{
    public int AffectedEnrollments { get; set; }
    public List<string> ImpactDescriptions { get; set; } = new();
    public bool WillLoseProgress { get; set; }
}

/// <summary>
/// Filter and pagination for listing edit requests
/// </summary>
public class EditRequestFilterDto
{
    public EditRequestStatus? Status { get; set; }
    public EditRequestType? RequestType { get; set; }
    public Guid? CourseId { get; set; }
    public Guid? InstructorId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "RequestedAt";
    public bool Descending { get; set; } = true;
}

/// <summary>
/// Payload for admin review action
/// </summary>
public class ReviewRequestDto
{
    public bool Approve { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Risk level for the edit policy engine
/// </summary>
public enum EditRiskLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
