using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend_project.DTOs.Section;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/management/courses/{courseId}/sections")]
[Authorize] // Instructors
public class SectionController : ControllerBase
{
    private readonly ISectionService _sectionService;

    public SectionController(ISectionService sectionService)
    {
        _sectionService = sectionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSectionsForCourse(Guid courseId)
    {
        var result = await _sectionService.GetSectionsForCourseAsync(courseId);
        return Ok(backend_project.DTOs.ApiResponse<System.Collections.Generic.List<SectionDto>>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<IActionResult> CreateSection(Guid courseId, [FromBody] CreateSectionDto dto)
    {
        var userId = GetUserId();
        var result = await _sectionService.CreateSectionAsync(courseId, userId, dto);
        return CreatedAtAction(nameof(GetSectionById), new { courseId, sectionId = result.Id }, backend_project.DTOs.ApiResponse<SectionDto>.SuccessResponse(result));
    }

    [HttpGet("{sectionId}")]
    public async Task<IActionResult> GetSectionById(Guid courseId, Guid sectionId)
    {
        var result = await _sectionService.GetSectionByIdAsync(sectionId);
        return Ok(backend_project.DTOs.ApiResponse<SectionDto>.SuccessResponse(result));
    }

    [HttpPut("{sectionId}")]
    public async Task<IActionResult> UpdateSection(Guid courseId, Guid sectionId, [FromBody] UpdateSectionDto dto)
    {
        var userId = GetUserId();
        var result = await _sectionService.UpdateSectionAsync(sectionId, userId, dto);
        return Ok(backend_project.DTOs.ApiResponse<SectionDto>.SuccessResponse(result));
    }

    [HttpDelete("{sectionId}")]
    public async Task<IActionResult> DeleteSection(Guid courseId, Guid sectionId)
    {
        var userId = GetUserId();
        await _sectionService.DeleteSectionAsync(sectionId, userId);
        return Ok(backend_project.DTOs.ApiResponse<object>.SuccessResponse(null, "Section deleted successfully"));
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> ReorderSections(Guid courseId, [FromBody] ReorderRequestDto dto)
    {
        var userId = GetUserId();
        await _sectionService.ReorderSectionsAsync(courseId, userId, dto);
        return Ok(backend_project.DTOs.ApiResponse<object>.SuccessResponse(null, "Sections reordered successfully"));
    }

    // --- Section Items ---

    [HttpPost("{sectionId}/items")]
    public async Task<IActionResult> AddItemToSection(Guid courseId, Guid sectionId, [FromBody] CreateSectionItemDto dto)
    {
        var userId = GetUserId();
        var result = await _sectionService.AddItemToSectionAsync(sectionId, userId, dto);
        return Ok(backend_project.DTOs.ApiResponse<SectionItemDto>.SuccessResponse(result));
    }

    [HttpPut("{sectionId}/items/{itemId}")]
    public async Task<IActionResult> UpdateSectionItem(Guid courseId, Guid sectionId, Guid itemId, [FromBody] UpdateSectionItemDto dto)
    {
        var userId = GetUserId();
        var result = await _sectionService.UpdateSectionItemAsync(itemId, userId, dto);
        return Ok(backend_project.DTOs.ApiResponse<SectionItemDto>.SuccessResponse(result));
    }

    [HttpDelete("{sectionId}/items/{itemId}")]
    public async Task<IActionResult> DeleteSectionItem(Guid courseId, Guid sectionId, Guid itemId)
    {
        var userId = GetUserId();
        await _sectionService.DeleteSectionItemAsync(itemId, userId);
        return Ok(backend_project.DTOs.ApiResponse<object>.SuccessResponse(null, "Section item deleted successfully"));
    }

    [HttpPut("{sectionId}/items/reorder")]
    public async Task<IActionResult> ReorderSectionItems(Guid courseId, Guid sectionId, [FromBody] ReorderRequestDto dto)
    {
        var userId = GetUserId();
        await _sectionService.ReorderSectionItemsAsync(sectionId, userId, dto);
        return Ok(backend_project.DTOs.ApiResponse<object>.SuccessResponse(null, "Section items reordered successfully"));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(userIdClaim);
    }
}
