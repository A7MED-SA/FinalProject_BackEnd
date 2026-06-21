using Athary.Domain.Enums;

namespace Athary.Application.DTOs.Notification;

public sealed record CreateNotificationDto
{
    public Guid UserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Message { get; init; }
    public NotificationType Type { get; init; }
    public string? LinkUrl { get; init; }
    public string? Icon { get; init; }
}
