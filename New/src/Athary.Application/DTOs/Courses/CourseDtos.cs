using Athary.Domain.Enums;

namespace Athary.Application.DTOs.Courses;

public sealed record CreateCourseDto
{
    public string Title { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public string? Description { get; init; }
    public Guid CategoryId { get; init; }
    public CourseLevel Level { get; init; } = CourseLevel.Beginner;
    public CourseLanguage Language { get; init; } = CourseLanguage.Ar;
    public decimal Price { get; init; }
}

public sealed record UpdateCourseDto
{
    public string? Title { get; init; }
    public string? Slug { get; init; }
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
    public CourseLevel? Level { get; init; }
    public CourseLanguage? Language { get; init; }
    public decimal? Price { get; init; }
}

public sealed record CourseSummaryDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public CourseLevel Level { get; init; }
    public CourseLanguage Language { get; init; }
    public CourseStatus Status { get; init; }
    public int TotalDurationMinutes { get; init; }
    public int EnrollmentCount { get; init; }
    public decimal AverageRating { get; init; }
    public string? ThumbnailUrl { get; init; }
    public string? CategoryName { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed record CourseDetailsDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public Guid CreatedBy { get; init; }
    public string CreatorName { get; init; } = string.Empty;
    public CourseLevel Level { get; init; }
    public CourseLanguage Language { get; init; }
    public CourseStatus Status { get; init; }
    public decimal Price { get; init; }
    public string? ImageUrl { get; init; }
    public Guid? CourseImageFileId { get; init; }
    public int TotalDurationMinutes { get; init; }
    public int EnrollmentCount { get; init; }
    public decimal AverageRating { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime? ScheduledDeletionAt { get; init; }
    public string? DeletionReason { get; init; }
    public bool IsReadOnlyForStudents { get; init; }
    public string? RejectionReason { get; init; }
    public List<CourseRequirementDto> Requirements { get; init; } = new();
    public List<CourseLearningOutcomeDto> LearningOutcomes { get; init; } = new();
}

public sealed record CourseRequirementDto
{
    public Guid Id { get; init; }
    public string RequirementText { get; init; } = string.Empty;
}

public sealed record CourseLearningOutcomeDto
{
    public Guid Id { get; init; }
    public string OutcomeText { get; init; } = string.Empty;
}

public sealed record AddRequirementDto
{
    public string RequirementText { get; init; } = string.Empty;
}

public sealed record AddLearningOutcomeDto
{
    public string OutcomeText { get; init; } = string.Empty;
}

public sealed record SetCourseImageRequest
{
    public Guid FileId { get; init; }
}

public sealed record ScheduleDeletionDto
{
    public DateTime ScheduledDate { get; init; }
    public string? Reason { get; init; }
}

public sealed record ScheduledDeletionStatusDto
{
    public Guid CourseId { get; init; }
    public DateTime? ScheduledDeletionAt { get; init; }
    public string? DeletionReason { get; init; }
    public bool IsReadOnlyForStudents { get; init; }
    public bool IsPending => ScheduledDeletionAt.HasValue && !IsReadOnlyForStudents;
    public bool IsExecuted => IsReadOnlyForStudents;
    public int EnrolledStudentCount { get; init; }
}

public sealed record CreateEnrollmentDto
{
    public Guid CourseId { get; init; }
    public Guid UserId { get; init; }
    public EnrollmentSource Source { get; init; } = EnrollmentSource.Purchase;
}

public sealed record EnrollmentResponseDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid CourseId { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public DateTime EnrolledAt { get; init; }
    public EnrollmentStatus Status { get; init; }
    public decimal ProgressPercentage { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime? LastAccessedAt { get; init; }
    public EnrollmentSource Source { get; init; }
    public DateTime? AccessExpiresAt { get; init; }
    public bool IsRefunded { get; init; }
}

public sealed record EnrollmentDetailDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid CourseId { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public DateTime EnrolledAt { get; init; }
    public EnrollmentStatus Status { get; init; }
    public decimal ProgressPercentage { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime? LastAccessedAt { get; init; }
    public EnrollmentSource Source { get; init; }
    public DateTime? AccessExpiresAt { get; init; }
    public bool IsRefunded { get; init; }
    public IEnumerable<ContentProgressDto> Progresses { get; init; } = new List<ContentProgressDto>();
}

public sealed record ContentProgressDto
{
    public Guid Id { get; init; }
    public Guid EnrollmentId { get; init; }
    public ContentType ContentType { get; init; }
    public Guid ContentId { get; init; }
    public bool IsCompleted { get; init; }
    public int WatchTimeSeconds { get; init; }
    public int AttemptsCount { get; init; }
    public decimal CompletionPercentage { get; init; }
    public string? Metadata { get; init; }
    public DateTime? LastAccessedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

public sealed record UpdateProgressDto
{
    public int WatchTimeSeconds { get; init; }
    public decimal? CompletionPercentage { get; init; }
    public string? Metadata { get; init; }
    public bool MarkAsCompleted { get; init; }
}
