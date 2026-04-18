using backend_project.Models;

namespace backend_project.DTOs.Notifications;

public class MarkAsReadDto
{
    public Guid NotificationId { get; set; }
}