using backend_project.Models;
using System.ComponentModel.DataAnnotations;

namespace backend_project.DTOs.Notifications;

public class CreateNotificationDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Message { get; set; }

    public NotificationType Type { get; set; }

    [StringLength(500)]
    public string? LinkUrl { get; set; }

    [StringLength(50)]
    public string? Icon { get; set; }
}