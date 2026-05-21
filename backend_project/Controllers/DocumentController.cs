using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.DTOs.Document;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/courses/{courseId}/documents")]
[Authorize(Roles = "Instructor")]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocument(Guid courseId, Guid id)
    {
        try
        {
            var result = await _documentService.GetDocumentAsync(id);
            return Ok(ApiResponse<DocumentResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DocumentResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateDocument(Guid courseId, [FromBody] CreateDocumentDto createDto)
    {
        try
        {
            var result = await _documentService.CreateDocumentAsync(createDto);
            return CreatedAtAction(nameof(GetDocument), new { courseId, id = result.Id }, ApiResponse<DocumentResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DocumentResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDocument(Guid courseId, Guid id, [FromBody] UpdateDocumentDto updateDto)
    {
        try
        {
            var result = await _documentService.UpdateDocumentAsync(id, updateDto);
            return Ok(ApiResponse<DocumentResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DocumentResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(Guid courseId, Guid id)
    {
        var result = await _documentService.DeleteDocumentAsync(id);
        if (!result)
            return NotFound(ApiResponse<object>.FailureResponse("Document not found."));

        return Ok(ApiResponse<object>.SuccessResponse(new { message = "Deleted" }));
    }

    private Guid GetUserId()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        return userId;
    }
}
