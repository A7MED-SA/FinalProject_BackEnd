namespace Athary.Application.DTOs.Communication;

// ─── Announcements ──────────────────────────────────────────────────

public sealed record CreateAnnouncementRequest
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Target { get; init; } = "All";
    public Guid? CourseId { get; init; }
}

public sealed record UpdateAnnouncementRequest
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Target { get; init; } = "All";
    public Guid? CourseId { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed record AnnouncementResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public Guid? CourseId { get; init; }
    public Guid CreatedBy { get; init; }
    public string CreatedByName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime? PublishedAt { get; init; }
}

public sealed record AnnouncementListResponse
{
    public List<AnnouncementResponse> Items { get; init; } = new();
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int TotalCount { get; init; }
}

// ─── Reports ────────────────────────────────────────────────────────

public sealed record CreateReportRequest
{
    public string EntityType { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public sealed record ResolveReportRequest
{
    public string Status { get; init; } = "Dismissed";
    public string? AdminNote { get; init; }
}

public sealed record ReportResponse
{
    public Guid Id { get; init; }
    public Guid ReporterId { get; init; }
    public string ReporterName { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = "Pending";
    public string? AdminNote { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
}

public sealed record ReportListResponse
{
    public List<ReportResponse> Items { get; init; } = new();
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int TotalCount { get; init; }
}
