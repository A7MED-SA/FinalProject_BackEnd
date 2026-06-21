namespace Athary.Application.DTOs.Notification;

public sealed record NotificationListDto
{
    public List<NotificationDto> Notifications { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}
