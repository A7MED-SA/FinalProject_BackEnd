using System.ComponentModel.DataAnnotations;

namespace Athary.Domain.Entities;

public class LegalPage : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public bool IsPublished { get; set; } = true;

    [StringLength(20)]
    public string? Version { get; set; }

    public Guid? LastUpdatedById { get; set; }
    public User? LastUpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
