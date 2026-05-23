using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.DTOs.Refund;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/refunds")]
[Authorize]
public class RefundController : ControllerBase
{
    private readonly IRefundService _refundService;

    public RefundController(IRefundService refundService)
    {
        _refundService = refundService;
    }

    private Guid GetUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(id!);
    }

    [HttpPost]
    public async Task<IActionResult> RequestRefund([FromBody] RequestRefundRequest request)
    {
        try
        {
            var userId = GetUserId();
            var refund = await _refundService.RequestRefundAsync(userId, request);
            return Ok(ApiResponse<RefundResponseDto>.SuccessResponse(refund, "Refund requested."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyRefunds()
    {
        var userId = GetUserId();
        var refunds = await _refundService.GetUserRefundsAsync(userId);
        return Ok(ApiResponse<IEnumerable<RefundResponseDto>>.SuccessResponse(refunds));
    }
}
