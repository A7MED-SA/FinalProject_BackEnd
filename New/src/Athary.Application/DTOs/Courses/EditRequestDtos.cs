using Athary.Domain.Enums;

namespace Athary.Application.DTOs.Courses;

public sealed record EditResultDto
{
    public bool AppliedImmediately { get; init; }
    public Guid? RequestId { get; init; }
    public EditRequestStatus? Status { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;
}

public sealed record EditRequestSummaryDto
{
    public Guid Id { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public string InstructorName { get; init; } = string.Empty;
    public EditRequestType RequestType { get; init; }
    public EditOperation Operation { get; init; }
    public EditRequestStatus Status { get; init; }
    public DateTime RequestedAt { get; init; }
    public TimeSpan TimeUntilExpiry { get; init; }
    public bool IsEmergency { get; init; }
}

public sealed record EditRequestDetailDto
{
    public Guid RequestId { get; init; }
    public Guid CourseId { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public Guid InstructorId { get; init; }
    public string InstructorName { get; init; } = string.Empty;
    public EditRequestType TargetType { get; init; }
    public EditOperation Operation { get; init; }
    public List<FieldChangeDto> Changes { get; init; } = new();
    public DateTime RequestedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public EditRiskLevel RiskLevel { get; init; }
}

public sealed record FieldChangeDto
{
    public string FieldName { get; init; } = string.Empty;
    public string FieldLabel { get; init; } = string.Empty;
    public object? OldValue { get; init; }
    public object? NewValue { get; init; }
    public ChangeType ChangeType { get; init; }
}

public enum ChangeType { Added, Modified, Deleted }

public sealed record EditRequestFilterDto
{
    public EditRequestStatus? Status { get; init; }
    public EditRequestType? RequestType { get; init; }
    public Guid? CourseId { get; init; }
    public Guid? InstructorId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed record ReviewRequestDto
{
    public bool Approve { get; init; }
    public string? Notes { get; init; }
}

public enum EditRiskLevel
{
    Low,
    Medium,
    High,
    Critical
}
