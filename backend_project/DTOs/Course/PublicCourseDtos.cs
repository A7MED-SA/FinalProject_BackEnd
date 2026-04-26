using backend_project.Models;

namespace backend_project.DTOs.Course;

/// <summary>
/// Lightweight DTO for public course listing cards.
/// Omits all sensitive data (video URLs, quiz answers, etc.)
/// </summary>
public class PublicCourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CourseImageUrl { get; set; }
    public decimal Price { get; set; }
    public bool IsFree => Price == 0;
    public CourseLevel Level { get; set; }
    public CourseLanguage Language { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public string? InstructorImageUrl { get; set; }
    public decimal AverageRating { get; set; }
    public int EnrollmentCount { get; set; }
    public int TotalDurationMinutes { get; set; }
    public int SectionCount { get; set; }
    public int LessonCount { get; set; }
    public DateTime? PublishedAt { get; set; }
}

/// <summary>
/// Detailed DTO for the public course detail page.
/// Includes curriculum outline (section titles + item types) but NOT actual content.
/// </summary>
public class PublicCourseDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CourseImageUrl { get; set; }
    public string? IntroVideoUrl { get; set; }
    public decimal Price { get; set; }
    public bool IsFree => Price == 0;
    public CourseLevel Level { get; set; }
    public CourseLanguage Language { get; set; }

    // Category Info
    public string CategoryName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }

    // Instructor Info
    public PublicInstructorDto Instructor { get; set; } = null!;

    // Stats
    public decimal AverageRating { get; set; }
    public int EnrollmentCount { get; set; }
    public int TotalDurationMinutes { get; set; }

    // Content outline
    public List<string> Requirements { get; set; } = new();
    public List<string> LearningOutcomes { get; set; } = new();
    public List<PublicSectionDto> Sections { get; set; } = new();

    // Metadata
    public int Version { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? LastContentUpdateAt { get; set; }
}

/// <summary>
/// Public instructor info (no sensitive data)
/// </summary>
public class PublicInstructorDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }
    public int TotalCourses { get; set; }
    public int TotalStudents { get; set; }
}

/// <summary>
/// Section outline shown on public pages (title + items list, no content)
/// </summary>
public class PublicSectionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Position { get; set; }
    public List<PublicSectionItemDto> Items { get; set; } = new();
}

/// <summary>
/// Section item summary (type + preview flag only)
/// </summary>
public class PublicSectionItemDto
{
    public Guid Id { get; set; }
    public SectionItemType ItemType { get; set; }
    public int Position { get; set; }
    public bool IsPreviewAllowed { get; set; }
    public string? Title { get; set; }
    public int? DurationMinutes { get; set; }
}

/// <summary>
/// Filter/search parameters for public course listing
/// </summary>
public class PublicCourseFilterDto
{
    public string? SearchQuery { get; set; }
    public Guid? CategoryId { get; set; }
    public CourseLevel? Level { get; set; }
    public CourseLanguage? Language { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsFreeOnly { get; set; }
    public decimal? MinRating { get; set; }
    public PublicCourseSortBy SortBy { get; set; } = PublicCourseSortBy.PublishedAt;
    public bool SortDescending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public enum PublicCourseSortBy
{
    PublishedAt,
    Price,
    AverageRating,
    EnrollmentCount,
    Title
}

/// <summary>
/// Autocomplete suggestion for quick search
/// </summary>
public class CourseSuggestionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? CourseImageUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

/// <summary>
/// Platform-wide statistics shown on landing page
/// </summary>
public class PlatformStatsDto
{
    public int TotalCourses { get; set; }
    public int TotalStudents { get; set; }
    public int TotalInstructors { get; set; }
    public int TotalCategories { get; set; }
}

/// <summary>
/// Filter dropdown options for the UI
/// </summary>
public class FilterOptionsDto
{
    public List<FilterOptionItem> Categories { get; set; } = new();
    public List<CourseLevel> Levels { get; set; } = new();
    public List<CourseLanguage> Languages { get; set; } = new();
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
}

public class FilterOptionItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CourseCount { get; set; }
}
