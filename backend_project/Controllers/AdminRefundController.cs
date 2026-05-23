using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.DTOs.Refund;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/admin/refunds")]
[Authorize(Roles = "Admin")]
public class AdminRefundController : ControllerBase
{
    private readonly IRefundService _refundService;

    public AdminRefundController(IRefundService refundService)
    {
        _refundService = refundService;
    }

    private Guid GetUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(id!);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        Models.RefundStatus? filter = status switch
        {
            "Requested" => Models.RefundStatus.Requested,
            "Approved" => Models.RefundStatus.Approved,
            "Rejected" => Models.RefundStatus.Rejected,
            _ => null
        };
        var refunds = await _refundService.GetAllRefundsAsync(filter);
        return Ok(ApiResponse<IEnumerable<RefundResponseDto>>.SuccessResponse(refunds));
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        try
        {
            var adminId = GetUserId();
            var refund = await _refundService.ApproveRefundAsync(adminId, new ProcessRefundRequest { RefundId = id });
            return Ok(ApiResponse<RefundResponseDto>.SuccessResponse(refund, "Refund approved."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        try
        {
            var adminId = GetUserId();
            var refund = await _refundService.RejectRefundAsync(adminId, new ProcessRefundRequest { RefundId = id });
            return Ok(ApiResponse<RefundResponseDto>.SuccessResponse(refund, "Refund rejected."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }
}
