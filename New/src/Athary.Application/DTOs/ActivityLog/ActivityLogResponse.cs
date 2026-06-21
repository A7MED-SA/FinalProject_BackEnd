namespace Athary.Application.DTOs.ActivityLog;

public sealed record ActivityLogResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public Guid? EntityId { get; init; }
    public string? Details { get; init; }
    public string? IpAddress { get; init; }
    public DateTime CreatedAt { get; init; }
}
