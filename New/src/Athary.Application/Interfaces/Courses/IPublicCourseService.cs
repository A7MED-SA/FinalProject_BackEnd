using Athary.Application.Common;
using Athary.Application.DTOs.Courses;

namespace Athary.Application.Interfaces.Courses;

public interface IPublicCourseService
{
    Task<PagedList<PublicCourseDto>> GetPublishedCoursesAsync(PublicCourseFilterDto filter, CancellationToken cancellationToken = default);

    Task<PublicCourseDetailDto> GetCourseDetailsAsync(Guid courseId, CancellationToken cancellationToken = default);

    Task<PublicCourseDetailDto> GetCourseDetailsBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<List<CourseSuggestionDto>> SearchCoursesSuggestAsync(string query, int limit = 5, CancellationToken cancellationToken = default);

    Task<PlatformStatsDto> GetPlatformStatsAsync(CancellationToken cancellationToken = default);

    Task<List<PublicCourseDto>> GetRelatedCoursesAsync(Guid courseId, int limit = 4, CancellationToken cancellationToken = default);

    Task<FilterOptionsDto> GetFilterOptionsAsync(CancellationToken cancellationToken = default);
}
