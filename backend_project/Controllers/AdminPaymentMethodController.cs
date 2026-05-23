using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.DTOs.Payment;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/admin/payment-methods")]
[Authorize(Roles = "Admin")]
public class AdminPaymentMethodController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public AdminPaymentMethodController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var methods = await _paymentService.GetActivePaymentMethodsAsync();
        return Ok(ApiResponse<IEnumerable<PaymentMethodResponse>>.SuccessResponse(methods));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentMethodRequest request)
    {
        try
        {
            var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value!);
            var method = await _paymentService.CreatePaymentMethodAsync(adminId, request);
            return Ok(ApiResponse<PaymentMethodResponse>.SuccessResponse(method, "Payment method created."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle(Guid id)
    {
        var result = await _paymentService.TogglePaymentMethodAsync(id);
        if (!result)
            return NotFound(ApiResponse<object>.FailureResponse("Payment method not found."));

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Payment method toggled."));
    }
}
