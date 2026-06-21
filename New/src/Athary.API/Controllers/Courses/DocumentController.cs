using Athary.Application.Common;
using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Athary.API.Controllers.Courses;

[ApiController]
[Route("api/courses/{courseId:guid}/documents")]
[Authorize(Roles = "Instructor")]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DocumentResponseDto>>> GetDocument(
        Guid courseId,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _documentService.GetDocumentAsync(id, cancellationToken);
            return Ok(ApiResponse<DocumentResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DocumentResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<DocumentResponseDto>>> CreateDocument(
        Guid courseId,
        [FromBody] CreateDocumentDto createDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _documentService.CreateDocumentAsync(courseId, createDto, cancellationToken);
            return CreatedAtAction(nameof(GetDocument), new { courseId, id = result.Id }, ApiResponse<DocumentResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DocumentResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DocumentResponseDto>>> UpdateDocument(
        Guid courseId,
        Guid id,
        [FromBody] UpdateDocumentDto updateDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _documentService.UpdateDocumentAsync(id, updateDto, cancellationToken);
            return Ok(ApiResponse<DocumentResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DocumentResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDocument(
        Guid courseId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _documentService.DeleteDocumentAsync(id, cancellationToken);
        if (!result)
            return NotFound(ApiResponse<object>.FailureResponse("Document not found."));

        return Ok(ApiResponse<object>.SuccessResponse(new { message = "Deleted" }));
    }
}
