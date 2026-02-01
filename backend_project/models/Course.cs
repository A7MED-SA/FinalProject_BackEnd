using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("courses")]
public class Course : BaseEntity
{

    [Required]
    [Column("title")]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Column("slug")]
    [MaxLength(255)]
    public string Slug { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("course_image_file_id")]
    public Guid? CourseImageFileId { get; set; }

    [ForeignKey(nameof(CourseImageFileId))]
    public UploadedFile? CourseImageFile { get; set; }

    [Column("intro_video_file_id")]
    public Guid? IntroVideoFileId { get; set; }

    [ForeignKey(nameof(IntroVideoFileId))]
    public UploadedFile? IntroVideoFile { get; set; }


    [Column("price", TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    [Column("created_by")]
    public Guid CreatedBy { get; set; }

    [Required]
    [Column("category_id")]
    public Guid CategoryId { get; set; }

    [Column("level")]
    [MaxLength(20)]
    public CourseLevel Level { get; set; } = CourseLevel.Beginner;

    [Column("language")]
    [MaxLength(5)]
    public CourseLanguage Language { get; set; } = CourseLanguage.Ar;

    [Column("status")]
    [MaxLength(20)]
    public CourseStatus Status { get; set; } = CourseStatus.Draft;

    [Column("is_published")]
    public bool IsPublished { get; set; } = false;

    [Column("approved_by")]
    public Guid? ApprovedBy { get; set; }

    [Column("published_at")]
    public DateTime? PublishedAt { get; set; }

    [Column("archived_at")]
    public DateTime? ArchivedAt { get; set; }

    [Column("total_duration_minutes")]
    public int TotalDurationMinutes { get; set; } = 0;

    [Column("enrollment_count")]
    public int EnrollmentCount { get; set; } = 0;

    [Column("average_rating", TypeName = "decimal(3,2)")]
    public decimal AverageRating { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    // Navigation Properties
    [ForeignKey("CreatedBy")]
    public virtual User Creator { get; set; } = null!;

    [ForeignKey("ApprovedBy")]
    public virtual User? Approver { get; set; }

    [ForeignKey("CategoryId")]
    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<CourseRequirement> CourseRequirements { get; set; } = new List<CourseRequirement>();
    public virtual ICollection<CourseLearningOutcome> CourseLearningOutcomes { get; set; } = new List<CourseLearningOutcome>();
    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
}

public enum CourseLevel
{
    Beginner,
    Intermediate,
    Advanced
}

public enum CourseLanguage
{
    Ar,
    En
}

public enum CourseStatus
{
    Draft,
    PendingReview,
    Published,
    Archived
}
