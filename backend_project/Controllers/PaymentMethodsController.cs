using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.DTOs.Payment;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/payment-methods")]
public class PaymentMethodsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentMethodsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetActiveMethods()
    {
        var methods = await _paymentService.GetActivePaymentMethodsAsync();
        return Ok(ApiResponse<IEnumerable<PaymentMethodResponse>>.SuccessResponse(methods));
    }
}
