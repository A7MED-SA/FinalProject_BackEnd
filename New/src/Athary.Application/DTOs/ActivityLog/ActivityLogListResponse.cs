namespace Athary.Application.DTOs.ActivityLog;

public sealed record ActivityLogListResponse
{
    public IEnumerable<ActivityLogResponse> Items { get; init; } = new List<ActivityLogResponse>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}
