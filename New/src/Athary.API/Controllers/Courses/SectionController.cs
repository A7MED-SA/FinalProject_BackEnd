using Athary.Application.Common;
using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Athary.API.Controllers.Courses;

[ApiController]
[Route("api/management/courses/{courseId:guid}/sections")]
[Authorize]
public class SectionController : ControllerBase
{
    private readonly ISectionService _sectionService;

    public SectionController(ISectionService sectionService)
    {
        _sectionService = sectionService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SectionDto>>>> GetSectionsForCourse(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var result = await _sectionService.GetSectionsForCourseAsync(courseId, cancellationToken);
        return Ok(ApiResponse<List<SectionDto>>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SectionDto>>> CreateSection(
        Guid courseId,
        [FromBody] CreateSectionDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _sectionService.CreateSectionAsync(courseId, userId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetSectionById), new { courseId, sectionId = result.Id }, ApiResponse<SectionDto>.SuccessResponse(result));
    }

    [HttpGet("{sectionId:guid}")]
    public async Task<ActionResult<ApiResponse<SectionDto>>> GetSectionById(
        Guid courseId,
        Guid sectionId,
        CancellationToken cancellationToken)
    {
        var result = await _sectionService.GetSectionByIdAsync(sectionId, cancellationToken);
        return Ok(ApiResponse<SectionDto>.SuccessResponse(result));
    }

    [HttpPut("{sectionId:guid}")]
    public async Task<ActionResult<ApiResponse<SectionDto>>> UpdateSection(
        Guid courseId,
        Guid sectionId,
        [FromBody] UpdateSectionDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _sectionService.UpdateSectionAsync(sectionId, userId, dto, cancellationToken);
        return Ok(ApiResponse<SectionDto>.SuccessResponse(result));
    }

    [HttpDelete("{sectionId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSection(
        Guid courseId,
        Guid sectionId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _sectionService.DeleteSectionAsync(sectionId, userId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Section deleted successfully"));
    }

    [HttpPut("reorder")]
    public async Task<ActionResult<ApiResponse<object>>> ReorderSections(
        Guid courseId,
        [FromBody] ReorderRequestDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _sectionService.ReorderSectionsAsync(courseId, userId, dto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Sections reordered successfully"));
    }

    [HttpPost("{sectionId:guid}/items")]
    public async Task<ActionResult<ApiResponse<SectionItemDto>>> AddItemToSection(
        Guid courseId,
        Guid sectionId,
        [FromBody] CreateSectionItemDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _sectionService.AddItemToSectionAsync(sectionId, userId, dto, cancellationToken);
        return Ok(ApiResponse<SectionItemDto>.SuccessResponse(result));
    }

    [HttpPut("{sectionId:guid}/items/{itemId:guid}")]
    public async Task<ActionResult<ApiResponse<SectionItemDto>>> UpdateSectionItem(
        Guid courseId,
        Guid sectionId,
        Guid itemId,
        [FromBody] UpdateSectionItemDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _sectionService.UpdateSectionItemAsync(itemId, userId, dto, cancellationToken);
        return Ok(ApiResponse<SectionItemDto>.SuccessResponse(result));
    }

    [HttpDelete("{sectionId:guid}/items/{itemId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSectionItem(
        Guid courseId,
        Guid sectionId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _sectionService.DeleteSectionItemAsync(itemId, userId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Section item deleted successfully"));
    }

    [HttpPut("{sectionId:guid}/items/reorder")]
    public async Task<ActionResult<ApiResponse<object>>> ReorderSectionItems(
        Guid courseId,
        Guid sectionId,
        [FromBody] ReorderRequestDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _sectionService.ReorderSectionItemsAsync(sectionId, userId, dto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Section items reordered successfully"));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(userIdClaim);
    }
}
