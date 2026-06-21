using Athary.Application.Common;
using Athary.Application.DTOs.Commerce;
using Athary.Application.Interfaces.Commerce;
using Athary.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers.Commerce;

[ApiController]
[Route("api/admin/refunds")]
[Authorize(Roles = "Admin")]
public sealed class AdminRefundController : ControllerBase
{
    private readonly IRefundService _refundService;

    public AdminRefundController(IRefundService refundService)
    {
        _refundService = refundService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<RefundResponseDto>>>> GetAll([FromQuery] RefundStatus? status, CancellationToken cancellationToken)
    {
        var result = await _refundService.GetAllRefundsAsync(status, cancellationToken);
        return Ok(ApiResponse<IEnumerable<RefundResponseDto>>.SuccessResponse(result));
    }

    [HttpPost("approve")]
    public async Task<ActionResult<ApiResponse<RefundResponseDto>>> Approve([FromBody] ProcessRefundRequest request, CancellationToken cancellationToken)
    {
        var adminId = GetUserId();
        var result = await _refundService.ApproveRefundAsync(adminId, request, cancellationToken);
        return Ok(ApiResponse<RefundResponseDto>.SuccessResponse(result));
    }

    [HttpPost("reject")]
    public async Task<ActionResult<ApiResponse<RefundResponseDto>>> Reject([FromBody] ProcessRefundRequest request, CancellationToken cancellationToken)
    {
        var adminId = GetUserId();
        var result = await _refundService.RejectRefundAsync(adminId, request, cancellationToken);
        return Ok(ApiResponse<RefundResponseDto>.SuccessResponse(result));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (claim == null || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        return userId;
    }
}
