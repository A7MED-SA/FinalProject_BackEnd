using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class Course : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CourseImageFileId { get; set; }
    public UploadedFile? CourseImageFile { get; set; }
    public Guid? IntroVideoFileId { get; set; }
    public UploadedFile? IntroVideoFile { get; set; }
    public decimal Price { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid CategoryId { get; set; }
    public string? RejectionReason { get; set; }
    public CourseLevel Level { get; set; } = CourseLevel.Beginner;
    public CourseLanguage Language { get; set; } = CourseLanguage.Ar;
    public CourseStatus Status { get; set; } = CourseStatus.Draft;
    public bool IsPublished { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public int Version { get; set; } = 1;
    public DateTime? LastContentUpdateAt { get; set; }
    public int TotalDurationMinutes { get; set; }
    public int EnrollmentCount { get; set; }
    public decimal AverageRating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime? ScheduledDeletionAt { get; set; }
    public string? DeletionReason { get; set; }
    public bool IsReadOnlyForStudents { get; set; }

    public User Creator { get; set; } = null!;
    public User? Approver { get; set; }
    public Category Category { get; set; } = null!;
    public List<CourseRequirement> CourseRequirements { get; set; } = new List<CourseRequirement>();
    public List<CourseLearningOutcome> CourseLearningOutcomes { get; set; } = new List<CourseLearningOutcome>();
    public List<Section> Sections { get; set; } = new List<Section>();
}
