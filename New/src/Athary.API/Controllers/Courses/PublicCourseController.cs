using Athary.Application.Common;
using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers.Courses;

[ApiController]
[Route("api/public/courses")]
public class PublicCourseController : ControllerBase
{
    private readonly IPublicCourseService _publicCourseService;

    public PublicCourseController(IPublicCourseService publicCourseService)
    {
        _publicCourseService = publicCourseService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedList<PublicCourseDto>>>> GetPublishedCourses(
        [FromQuery] PublicCourseFilterDto filter,
        CancellationToken cancellationToken)
    {
        var result = await _publicCourseService.GetPublishedCoursesAsync(filter, cancellationToken);
        return Ok(ApiResponse<PagedList<PublicCourseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PublicCourseDetailDto>>> GetCourseById(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _publicCourseService.GetCourseDetailsAsync(id, cancellationToken);
            return Ok(ApiResponse<PublicCourseDetailDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PublicCourseDetailDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ApiResponse<PublicCourseDetailDto>>> GetCourseBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _publicCourseService.GetCourseDetailsBySlugAsync(slug, cancellationToken);
            return Ok(ApiResponse<PublicCourseDetailDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PublicCourseDetailDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("search/suggest")]
    public async Task<ActionResult<ApiResponse<List<CourseSuggestionDto>>>> SearchSuggest(
        [FromQuery] string query,
        CancellationToken cancellationToken,
        [FromQuery] int limit = 5)
    {
        var result = await _publicCourseService.SearchCoursesSuggestAsync(query, limit, cancellationToken);
        return Ok(ApiResponse<List<CourseSuggestionDto>>.SuccessResponse(result));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<PlatformStatsDto>>> GetPlatformStats(CancellationToken cancellationToken)
    {
        var result = await _publicCourseService.GetPlatformStatsAsync(cancellationToken);
        return Ok(ApiResponse<PlatformStatsDto>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}/related")]
    public async Task<ActionResult<ApiResponse<List<PublicCourseDto>>>> GetRelatedCourses(
        Guid id,
        CancellationToken cancellationToken,
        [FromQuery] int limit = 4)
    {
        var result = await _publicCourseService.GetRelatedCoursesAsync(id, limit, cancellationToken);
        return Ok(ApiResponse<List<PublicCourseDto>>.SuccessResponse(result));
    }

    [HttpGet("filters/options")]
    public async Task<ActionResult<ApiResponse<FilterOptionsDto>>> GetFilterOptions(CancellationToken cancellationToken)
    {
        var result = await _publicCourseService.GetFilterOptionsAsync(cancellationToken);
        return Ok(ApiResponse<FilterOptionsDto>.SuccessResponse(result));
    }
}
