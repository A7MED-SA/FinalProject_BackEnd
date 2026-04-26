using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs.Course;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/public/courses")]
public class PublicCourseController : ControllerBase
{
    private readonly IPublicCourseService _publicCourseService;

    public PublicCourseController(IPublicCourseService publicCourseService)
    {
        _publicCourseService = publicCourseService;
    }

    /// <summary>
    /// Get paginated list of published courses with optional filters
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPublishedCourses([FromQuery] PublicCourseFilterDto filter)
    {
        var result = await _publicCourseService.GetPublishedCoursesAsync(filter);
        return Ok(backend_project.DTOs.ApiResponse<PagedResult<PublicCourseDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get detailed info for a published course by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCourseById(Guid id)
    {
        var result = await _publicCourseService.GetCourseDetailsAsync(id);
        return Ok(backend_project.DTOs.ApiResponse<PublicCourseDetailDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Get detailed info for a published course by slug
    /// </summary>
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetCourseBySlug(string slug)
    {
        var result = await _publicCourseService.GetCourseDetailsBySlugAsync(slug);
        return Ok(backend_project.DTOs.ApiResponse<PublicCourseDetailDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Quick search autocomplete suggestions
    /// </summary>
    [HttpGet("search/suggest")]
    public async Task<IActionResult> SearchSuggest(
        [FromQuery] string query,
        [FromQuery] int limit = 5)
    {
        var result = await _publicCourseService.SearchCoursesSuggestAsync(query, limit);
        return Ok(backend_project.DTOs.ApiResponse<List<CourseSuggestionDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get platform-wide public statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetPlatformStats()
    {
        var result = await _publicCourseService.GetPlatformStatsAsync();
        return Ok(backend_project.DTOs.ApiResponse<PlatformStatsDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Get related courses based on category and level similarity
    /// </summary>
    [HttpGet("{id:guid}/related")]
    public async Task<IActionResult> GetRelatedCourses(Guid id, [FromQuery] int limit = 4)
    {
        var result = await _publicCourseService.GetRelatedCoursesAsync(id, limit);
        return Ok(backend_project.DTOs.ApiResponse<List<PublicCourseDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get available filter options (categories, levels, languages, price range)
    /// </summary>
    [HttpGet("filters/options")]
    public async Task<IActionResult> GetFilterOptions()
    {
        var result = await _publicCourseService.GetFilterOptionsAsync();
        return Ok(backend_project.DTOs.ApiResponse<FilterOptionsDto>.SuccessResponse(result));
    }
}
