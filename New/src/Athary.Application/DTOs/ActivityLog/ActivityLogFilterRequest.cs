namespace Athary.Application.DTOs.ActivityLog;

public sealed record ActivityLogFilterRequest
{
    public Guid? UserId { get; init; }
    public string? Action { get; init; }
    public string? EntityType { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public string? IpAddress { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
