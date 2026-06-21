using Athary.Domain.Enums;

namespace Athary.Application.DTOs.Notification;

public sealed record NotificationDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Message { get; init; }
    public NotificationType Type { get; init; }
    public string? LinkUrl { get; init; }
    public string? Icon { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ReadAt { get; init; }
}
