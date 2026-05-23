using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.DTOs.Cart;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private Guid GetUserId()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException();
        return userId;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = GetUserId();
        var cart = await _cartService.GetCartAsync(userId);
        return Ok(ApiResponse<CartResponseDto>.SuccessResponse(cart));
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddToCartRequest request)
    {
        try
        {
            var userId = GetUserId();
            var item = await _cartService.AddItemAsync(userId, request.CourseId);
            return CreatedAtAction(nameof(GetCart), null, ApiResponse<CartItemDto>.SuccessResponse(item, "Course added to cart."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("items/{itemId}")]
    public async Task<IActionResult> RemoveItem(Guid itemId)
    {
        try
        {
            var userId = GetUserId();
            await _cartService.RemoveItemAsync(userId, itemId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("coupon")]
    public async Task<IActionResult> ApplyCoupon([FromBody] DTOs.Cart.ApplyCouponRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _cartService.ApplyCouponAsync(userId, request.Code);
            return Ok(ApiResponse<ApplyCouponResponse>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("coupon")]
    public async Task<IActionResult> RemoveCoupon()
    {
        var userId = GetUserId();
        await _cartService.RemoveCouponAsync(userId);
        return NoContent();
    }
}
