using Athary.Application.Common;
using Athary.Application.DTOs.Commerce;
using Athary.Application.Interfaces.Commerce;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers.Commerce;

[ApiController]
[Route("api/orders")]
[Authorize]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponseDto>>>> GetOrders(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _orderService.GetOrdersAsync(userId, cancellationToken);
        return Ok(ApiResponse<IEnumerable<OrderResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<ApiResponse<OrderDetailDto>>> GetOrderDetails(Guid orderId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _orderService.GetOrderDetailsAsync(userId, orderId, cancellationToken);
        return Ok(ApiResponse<OrderDetailDto>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _orderService.CreateOrderAsync(userId, request, cancellationToken);
        return Ok(ApiResponse<OrderResponseDto>.SuccessResponse(result));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (claim == null || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        return userId;
    }
}
