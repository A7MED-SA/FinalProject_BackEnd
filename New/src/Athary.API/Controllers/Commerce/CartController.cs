using Athary.Application.Common;
using Athary.Application.DTOs.Commerce;
using Athary.Application.Interfaces.Commerce;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers.Commerce;

[ApiController]
[Route("api/cart")]
[Authorize]
public sealed class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CartResponseDto>>> GetCart(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _cartService.GetCartAsync(userId, cancellationToken);
        return Ok(ApiResponse<CartResponseDto>.SuccessResponse(result));
    }

    [HttpPost("items")]
    public async Task<ActionResult<ApiResponse<CartItemDto>>> AddItem([FromBody] AddToCartRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _cartService.AddItemAsync(userId, request.CourseId, cancellationToken);
        return Ok(ApiResponse<CartItemDto>.SuccessResponse(result));
    }

    [HttpDelete("items/{itemId}")]
    public async Task<IActionResult> RemoveItem(Guid itemId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _cartService.RemoveItemAsync(userId, itemId, cancellationToken);
        return NoContent();
    }

    [HttpPost("apply-coupon")]
    public async Task<ActionResult<ApiResponse<ApplyCouponResponse>>> ApplyCoupon([FromBody] ApplyCouponRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _cartService.ApplyCouponAsync(userId, request.Code, cancellationToken);
        return Ok(ApiResponse<ApplyCouponResponse>.SuccessResponse(result));
    }

    [HttpDelete("coupon")]
    public async Task<IActionResult> RemoveCoupon(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _cartService.RemoveCouponAsync(userId, cancellationToken);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (claim == null || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        return userId;
    }
}
