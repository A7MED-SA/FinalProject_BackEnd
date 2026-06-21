using Athary.Application.Common;
using Athary.Application.DTOs.Wishlist;
using Athary.Application.Interfaces.Wishlist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/wishlist")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<WishlistResponseDto>>> GetWishlist(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _wishlistService.GetWishlistAsync(userId, cancellationToken);
        return Ok(ApiResponse<WishlistResponseDto>.SuccessResponse(result));
    }

    [HttpPost("{courseId}")]
    public async Task<ActionResult<ApiResponse<WishlistItemDto>>> AddItem(Guid courseId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _wishlistService.AddItemAsync(userId, courseId, cancellationToken);
        return Ok(ApiResponse<WishlistItemDto>.SuccessResponse(result));
    }

    [HttpDelete("{courseId}")]
    public async Task<IActionResult> RemoveItem(Guid courseId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _wishlistService.RemoveItemAsync(userId, courseId, cancellationToken);
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
