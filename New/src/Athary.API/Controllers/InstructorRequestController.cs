using System.Security.Claims;
using Athary.Application.Common;
using Athary.Application.DTOs.InstructorRequests.Requests;
using Athary.Application.DTOs.InstructorRequests.Responses;
using Athary.Application.Interfaces.InstructorRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/instructor-requests")]
public class InstructorRequestController : ControllerBase
{
    private readonly IInstructorRequestService _service;

    public InstructorRequestController(IInstructorRequestService service)
    {
        _service = service;
    }

    [HttpGet("can-submit")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> CanSubmitRequest(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var canSubmit = await _service.CanSubmitRequestAsync(userId, cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResponse(canSubmit));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<InstructorRequestDto>>> SubmitRequest(
        [FromBody] SubmitInstructorRequestDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var request = await _service.SubmitRequestAsync(userId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetMyRequestById), new { requestId = request.Id }, ApiResponse<InstructorRequestDto>.SuccessResponse(request));
    }

    [HttpPut("{requestId:guid}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<InstructorRequestDto>>> UpdateRequest(
        Guid requestId,
        [FromBody] UpdateInstructorRequestDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var request = await _service.UpdateRequestAsync(requestId, userId, dto, cancellationToken);
        return Ok(ApiResponse<InstructorRequestDto>.SuccessResponse(request));
    }

    [HttpPost("{requestId:guid}/documents")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> AddDocument(
        Guid requestId,
        [FromBody] AddDocumentToRequestDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var success = await _service.AddDocumentAsync(requestId, userId, dto, cancellationToken);
        if (!success)
            return NotFound(ApiResponse<object>.FailureResponse("الطلب غير موجود أو لا يمكن تعديله"));

        return Ok(ApiResponse<object>.SuccessResponse(null!, "تم إضافة المستند بنجاح"));
    }

    [HttpGet("my-requests")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<InstructorRequestDto>>>> GetMyRequests(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var requests = await _service.GetMyRequestsAsync(userId, cancellationToken);
        return Ok(ApiResponse<List<InstructorRequestDto>>.SuccessResponse(requests));
    }

    [HttpGet("my-requests/{requestId:guid}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<InstructorRequestDetailDto>>> GetMyRequestById(Guid requestId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var request = await _service.GetMyRequestByIdAsync(userId, requestId, cancellationToken);
        return Ok(ApiResponse<InstructorRequestDetailDto>.SuccessResponse(request));
    }

    [HttpDelete("{requestId:guid}/cancel")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> CancelRequest(Guid requestId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var success = await _service.CancelRequestAsync(userId, requestId, cancellationToken);
        if (!success)
            return BadRequest(ApiResponse<object>.FailureResponse("لا يمكن إلغاء هذا الطلب"));

        return Ok(ApiResponse<object>.SuccessResponse(null!, "تم إلغاء الطلب بنجاح"));
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<InstructorRequestDto>>>> GetPendingRequests(CancellationToken cancellationToken)
    {
        var requests = await _service.GetPendingRequestsAsync(cancellationToken);
        return Ok(ApiResponse<List<InstructorRequestDto>>.SuccessResponse(requests));
    }

    [HttpGet("{requestId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<InstructorRequestDetailDto>>> GetRequestById(Guid requestId, CancellationToken cancellationToken)
    {
        var request = await _service.GetRequestByIdAsync(requestId, cancellationToken);
        return Ok(ApiResponse<InstructorRequestDetailDto>.SuccessResponse(request));
    }

    [HttpPut("{requestId:guid}/process")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<InstructorRequestDto>>> ProcessRequest(
        Guid requestId,
        [FromBody] ProcessInstructorRequestDto dto,
        CancellationToken cancellationToken)
    {
        var adminId = GetUserId();
        var request = await _service.ProcessRequestAsync(requestId, adminId, dto, cancellationToken);
        return Ok(ApiResponse<InstructorRequestDto>.SuccessResponse(request));
    }

    [HttpDelete("{requestId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteRequest(Guid requestId, CancellationToken cancellationToken)
    {
        var success = await _service.DeleteRequestAsync(requestId, cancellationToken);
        if (!success)
            return NotFound(ApiResponse<object>.FailureResponse("الطلب غير موجود"));

        return Ok(ApiResponse<object>.SuccessResponse(null!, "تم حذف الطلب بنجاح"));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(userIdClaim);
    }
}
