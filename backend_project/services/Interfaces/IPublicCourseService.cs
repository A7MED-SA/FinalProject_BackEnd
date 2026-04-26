using backend_project.DTOs.Course;

namespace backend_project.Services.Interfaces;

/// <summary>
/// Read-only service for public (unauthenticated) course browsing.
/// All queries use AsNoTracking for performance.
/// </summary>
public interface IPublicCourseService
{
    /// <summary>
    /// Get a paginated, filtered list of published courses.
    /// </summary>
    Task<PagedResult<PublicCourseDto>> GetPublishedCoursesAsync(PublicCourseFilterDto filter);

    /// <summary>
    /// Get full details for a published course by ID.
    /// </summary>
    Task<PublicCourseDetailDto> GetCourseDetailsAsync(Guid courseId);

    /// <summary>
    /// Get full details for a published course by slug.
    /// </summary>
    Task<PublicCourseDetailDto> GetCourseDetailsBySlugAsync(string slug);

    /// <summary>
    /// Quick search autocomplete suggestions.
    /// </summary>
    Task<List<CourseSuggestionDto>> SearchCoursesSuggestAsync(string query, int limit = 5);

    /// <summary>
    /// Get platform-wide public stats (total courses, students, etc.)
    /// </summary>
    Task<PlatformStatsDto> GetPlatformStatsAsync();

    /// <summary>
    /// Get related/similar courses for a given course.
    /// </summary>
    Task<List<PublicCourseDto>> GetRelatedCoursesAsync(Guid courseId, int limit = 4);

    /// <summary>
    /// Get available filter options (categories with counts, price ranges, etc.)
    /// </summary>
    Task<FilterOptionsDto> GetFilterOptionsAsync();
}
