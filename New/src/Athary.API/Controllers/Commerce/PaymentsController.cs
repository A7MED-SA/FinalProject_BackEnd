using Athary.Application.Common;
using Athary.Application.DTOs.Commerce;
using Athary.Application.Interfaces.Commerce;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers.Commerce;

[ApiController]
[Route("api/payments")]
[Authorize]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("process")]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> ProcessPayment(
        [FromQuery] Guid orderId, [FromBody] ProcessPaymentRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _paymentService.ProcessPaymentAsync(userId, orderId, request.PaymentMethodId, cancellationToken);
        return Ok(ApiResponse<PaymentResponseDto>.SuccessResponse(result));
    }

    [HttpGet("methods")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentMethodResponse>>>> GetPaymentMethods(CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetActivePaymentMethodsAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<PaymentMethodResponse>>.SuccessResponse(result));
    }

    [HttpGet("history/{orderId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentResponseDto>>>> GetPaymentHistory(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetPaymentHistoryAsync(orderId, cancellationToken);
        return Ok(ApiResponse<IEnumerable<PaymentResponseDto>>.SuccessResponse(result));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (claim == null || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        return userId;
    }
}
