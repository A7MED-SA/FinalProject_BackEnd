using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("course_requirements")]
public class CourseRequirement : BaseEntity
{

    [Required]
    [Column("course_id")]
    public Guid CourseId { get; set; }

    [Required]
    [Column("description")]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Column("display_order")]
    public int DisplayOrder { get; set; } = 0;

    // Navigation Properties
    [ForeignKey("CourseId")]
    public virtual Course Course { get; set; } = null!;
}
