using Athary.Domain.Enums;

namespace Athary.Application.DTOs.Courses;

public sealed record PublicCourseDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? CourseImageUrl { get; init; }
    public decimal Price { get; init; }
    public bool IsFree => Price == 0;
    public CourseLevel Level { get; init; }
    public CourseLanguage Language { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public string InstructorName { get; init; } = string.Empty;
    public decimal AverageRating { get; init; }
    public int EnrollmentCount { get; init; }
    public int TotalDurationMinutes { get; init; }
    public int SectionCount { get; init; }
    public int LessonCount { get; init; }
    public DateTime? PublishedAt { get; init; }
}

public sealed record PublicCourseDetailDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? CourseImageUrl { get; init; }
    public string? IntroVideoUrl { get; init; }
    public decimal Price { get; init; }
    public bool IsFree => Price == 0;
    public CourseLevel Level { get; init; }
    public CourseLanguage Language { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public PublicInstructorDto Instructor { get; init; } = null!;
    public decimal AverageRating { get; init; }
    public int EnrollmentCount { get; init; }
    public int TotalDurationMinutes { get; init; }
    public List<string> Requirements { get; init; } = new();
    public List<string> LearningOutcomes { get; init; } = new();
    public List<PublicSectionDto> Sections { get; init; } = new();
    public int Version { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime? LastContentUpdateAt { get; init; }
}

public sealed record PublicInstructorDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? Bio { get; init; }
    public string? ProfileImageUrl { get; init; }
}

public sealed record PublicSectionDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Position { get; init; }
    public List<PublicSectionItemDto> Items { get; init; } = new();
}

public sealed record PublicSectionItemDto
{
    public Guid Id { get; init; }
    public SectionItemType ItemType { get; init; }
    public int Position { get; init; }
    public bool IsPreviewAllowed { get; init; }
}

public sealed record PublicCourseFilterDto
{
    public string? SearchQuery { get; init; }
    public Guid? CategoryId { get; init; }
    public CourseLevel? Level { get; init; }
    public CourseLanguage? Language { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public bool? IsFreeOnly { get; init; }
    public decimal? MinRating { get; init; }
    public PublicCourseSortBy SortBy { get; init; } = PublicCourseSortBy.PublishedAt;
    public bool SortDescending { get; init; } = true;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 12;
}

public enum PublicCourseSortBy
{
    PublishedAt,
    Price,
    AverageRating,
    EnrollmentCount,
    Title
}

public sealed record CourseSuggestionDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
}

public sealed record PlatformStatsDto
{
    public int TotalCourses { get; init; }
    public int TotalStudents { get; init; }
    public int TotalInstructors { get; init; }
    public int TotalCategories { get; init; }
}

public sealed record FilterOptionsDto
{
    public List<FilterOptionItem> Categories { get; init; } = new();
    public List<CourseLevel> Levels { get; init; } = new();
    public List<CourseLanguage> Languages { get; init; } = new();
    public decimal MinPrice { get; init; }
    public decimal MaxPrice { get; init; }
}

public sealed record FilterOptionItem
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int CourseCount { get; init; }
}
