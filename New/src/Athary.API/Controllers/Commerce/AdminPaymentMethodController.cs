using Athary.Application.Common;
using Athary.Application.DTOs.Commerce;
using Athary.Application.Interfaces.Commerce;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers.Commerce;

[ApiController]
[Route("api/admin/payment-methods")]
[Authorize(Roles = "Admin")]
public sealed class AdminPaymentMethodController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public AdminPaymentMethodController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentMethodResponse>>>> GetAll(CancellationToken cancellationToken)
    {
        var methods = await _paymentService.GetActivePaymentMethodsAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<PaymentMethodResponse>>.SuccessResponse(methods));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PaymentMethodResponse>>> Create([FromBody] CreatePaymentMethodRequest request, CancellationToken cancellationToken)
    {
        var adminId = GetUserId();
        var method = await _paymentService.CreatePaymentMethodAsync(adminId, request, cancellationToken);
        return Ok(ApiResponse<PaymentMethodResponse>.SuccessResponse(method));
    }

    [HttpPatch("{id}/toggle")]
    public async Task<ActionResult<ApiResponse<bool>>> Toggle(Guid id, CancellationToken cancellationToken)
    {
        var result = await _paymentService.TogglePaymentMethodAsync(id, cancellationToken);
        if (!result)
            return NotFound(ApiResponse<bool>.FailureResponse("طريقة الدفع غير موجودة."));
        return Ok(ApiResponse<bool>.SuccessResponse(result));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (claim == null || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        return userId;
    }
}
