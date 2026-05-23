using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.DTOs.Order;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;

    public OrderController(IOrderService orderService, IPaymentService paymentService)
    {
        _orderService = orderService;
        _paymentService = paymentService;
    }

    private Guid GetUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var userId))
            throw new UnauthorizedAccessException();
        return userId;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var userId = GetUserId();
            var order = await _orderService.CreateOrderAsync(userId, request);
            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, ApiResponse<OrderResponseDto>.SuccessResponse(order, "Order created."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var userId = GetUserId();
        var orders = await _orderService.GetOrdersAsync(userId);
        return Ok(ApiResponse<IEnumerable<OrderResponseDto>>.SuccessResponse(orders));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var order = await _orderService.GetOrderDetailsAsync(userId, id);
            return Ok(ApiResponse<OrderDetailDto>.SuccessResponse(order));
        }
        catch (Exception ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("{orderId}/payments")]
    public async Task<IActionResult> ProcessPayment(Guid orderId, [FromBody] DTOs.Payment.ProcessPaymentRequest request)
    {
        try
        {
            var userId = GetUserId();
            var payment = await _paymentService.ProcessPaymentAsync(userId, orderId, request.PaymentMethodId);
            return Ok(ApiResponse<DTOs.Payment.PaymentResponseDto>.SuccessResponse(payment));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("{orderId}/payments")]
    public async Task<IActionResult> GetPaymentHistory(Guid orderId)
    {
        try
        {
            var payments = await _paymentService.GetPaymentHistoryAsync(orderId);
            return Ok(ApiResponse<IEnumerable<DTOs.Payment.PaymentResponseDto>>.SuccessResponse(payments));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }
}
