using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend_project.DTOs.Section;
using backend_project.DTOs.EditRequest;
using backend_project.Helpers;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/management/courses/{courseId}/sections")]
[Authorize] // Instructors
public class SectionController : ControllerBase
{
    private readonly ISectionService _sectionService;
    private readonly ICourseEditApprovalService _editApprovalService;

    public SectionController(
        ISectionService sectionService,
        ICourseEditApprovalService editApprovalService)
    {
        _sectionService = sectionService;
        _editApprovalService = editApprovalService;
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

        // Create edit context for the policy engine
        var context = new EditContext
        {
            TargetType = EditRequestType.Section,
            Operation = EditOperation.Update,
            TargetEntityId = sectionId
        };

        var payload = JsonSerializer.Serialize(dto);

        var editResult = await _editApprovalService.RequestEditAsync(
            courseId, userId, context, payload);

        if (editResult.AppliedImmediately)
        {
            // Policy allows immediate update (non-published or low-risk)
            var result = await _sectionService.UpdateSectionAsync(sectionId, userId, dto);
            return Ok(backend_project.DTOs.ApiResponse<SectionDto>.SuccessResponse(result));
        }

        // Requires admin approval
        return Accepted(backend_project.DTOs.ApiResponse<EditResultDto>.SuccessResponse(
            editResult, "Edit request submitted for admin approval"));
    }

    [HttpDelete("{sectionId}")]
    public async Task<IActionResult> DeleteSection(Guid courseId, Guid sectionId)
    {
        var userId = GetUserId();

        var context = new EditContext
        {
            TargetType = EditRequestType.Section,
            Operation = EditOperation.Delete,
            TargetEntityId = sectionId
        };

        var editResult = await _editApprovalService.RequestEditAsync(
            courseId, userId, context, null);

        if (editResult.AppliedImmediately)
        {
            await _sectionService.DeleteSectionAsync(sectionId, userId);
            return Ok(backend_project.DTOs.ApiResponse<object>.SuccessResponse(null, "Section deleted successfully"));
        }

        return Accepted(backend_project.DTOs.ApiResponse<EditResultDto>.SuccessResponse(
            editResult, "Delete request submitted for admin approval"));
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

        var context = new EditContext
        {
            TargetType = EditRequestType.SectionItem,
            Operation = EditOperation.Update,
            TargetEntityId = itemId
        };

        var payload = JsonSerializer.Serialize(dto);

        var editResult = await _editApprovalService.RequestEditAsync(
            courseId, userId, context, payload);

        if (editResult.AppliedImmediately)
        {
            var result = await _sectionService.UpdateSectionItemAsync(itemId, userId, dto);
            return Ok(backend_project.DTOs.ApiResponse<SectionItemDto>.SuccessResponse(result));
        }

        return Accepted(backend_project.DTOs.ApiResponse<EditResultDto>.SuccessResponse(
            editResult, "Edit request submitted for admin approval"));
    }

    [HttpDelete("{sectionId}/items/{itemId}")]
    public async Task<IActionResult> DeleteSectionItem(Guid courseId, Guid sectionId, Guid itemId)
    {
        var userId = GetUserId();

        var context = new EditContext
        {
            TargetType = EditRequestType.SectionItem,
            Operation = EditOperation.Delete,
            TargetEntityId = itemId
        };

        var editResult = await _editApprovalService.RequestEditAsync(
            courseId, userId, context, null);

        if (editResult.AppliedImmediately)
        {
            await _sectionService.DeleteSectionItemAsync(itemId, userId);
            return Ok(backend_project.DTOs.ApiResponse<object>.SuccessResponse(null, "Section item deleted successfully"));
        }

        return Accepted(backend_project.DTOs.ApiResponse<EditResultDto>.SuccessResponse(
            editResult, "Delete request submitted for admin approval"));
    }

    [HttpPut("{sectionId}/items/reorder")]
    public async Task<IActionResult> ReorderSectionItems(Guid courseId, Guid sectionId, [FromBody] ReorderRequestDto dto)
    {
        var userId = GetUserId();
        await _sectionService.ReorderSectionItemsAsync(sectionId, userId, dto);
        return Ok(backend_project.DTOs.ApiResponse<object>.SuccessResponse(null, "Section items reordered successfully"));
    }

    // --- Instructor Edit Requests ---

    [HttpGet("/api/courses/my-edit-requests")]
    public async Task<IActionResult> GetMyEditRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var filter = new EditRequestFilterDto
        {
            InstructorId = userId,
            Page = page,
            PageSize = pageSize,
            Status = null // Show all statuses for instructor
        };
        var result = await _editApprovalService.GetPendingRequestsAsync(filter);
        return Ok(backend_project.DTOs.ApiResponse<PagedResult<EditRequestSummaryDto>>.SuccessResponse(result));
    }

    [HttpPost("/api/courses/edit-requests/{requestId}/cancel")]
    public async Task<IActionResult> CancelEditRequest(Guid requestId)
    {
        var userId = GetUserId();
        var success = await _editApprovalService.CancelRequestAsync(requestId, userId);
        if (!success)
            return NotFound(backend_project.DTOs.ApiResponse<object>.FailureResponse("Edit request not found or cannot be cancelled"));

        return Ok(backend_project.DTOs.ApiResponse<object>.SuccessResponse(null, "Edit request cancelled successfully"));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(userIdClaim);
    }
}
