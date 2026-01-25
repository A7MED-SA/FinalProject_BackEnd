using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("sections")]
public class Section : BaseEntity
{

    [Required]
    [Column("course_id")]
    public Guid CourseId { get; set; }

    [Required]
    [Column("title")]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("position")]
    public int Position { get; set; } = 0;

    [Column("is_locked")]
    public bool IsLocked { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("CourseId")]
    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<SectionItem> SectionItems { get; set; } = new List<SectionItem>();
}
