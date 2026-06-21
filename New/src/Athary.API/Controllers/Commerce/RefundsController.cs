using Athary.Application.Common;
using Athary.Application.DTOs.Commerce;
using Athary.Application.Interfaces.Commerce;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers.Commerce;

[ApiController]
[Route("api/refunds")]
[Authorize]
public sealed class RefundsController : ControllerBase
{
    private readonly IRefundService _refundService;

    public RefundsController(IRefundService refundService)
    {
        _refundService = refundService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RefundResponseDto>>> RequestRefund([FromBody] RequestRefundRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _refundService.RequestRefundAsync(userId, request, cancellationToken);
        return Ok(ApiResponse<RefundResponseDto>.SuccessResponse(result));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<RefundResponseDto>>>> GetMyRefunds(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _refundService.GetUserRefundsAsync(userId, cancellationToken);
        return Ok(ApiResponse<IEnumerable<RefundResponseDto>>.SuccessResponse(result));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (claim == null || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        return userId;
    }
}
