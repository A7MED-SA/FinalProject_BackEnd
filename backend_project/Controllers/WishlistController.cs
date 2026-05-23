using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.DTOs.Wishlist;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

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

    private Guid GetUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(id!);
    }

    [HttpGet]
    public async Task<IActionResult> GetWishlist()
    {
        var userId = GetUserId();
        var wishlist = await _wishlistService.GetWishlistAsync(userId);
        return Ok(ApiResponse<WishlistResponseDto>.SuccessResponse(wishlist));
    }

    [HttpPost("{courseId}")]
    public async Task<IActionResult> AddItem(Guid courseId)
    {
        try
        {
            var userId = GetUserId();
            var item = await _wishlistService.AddItemAsync(userId, courseId);
            return Ok(ApiResponse<WishlistItemDto>.SuccessResponse(item, "Course added to wishlist."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{courseId}")]
    public async Task<IActionResult> RemoveItem(Guid courseId)
    {
        try
        {
            var userId = GetUserId();
            await _wishlistService.RemoveItemAsync(userId, courseId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }
}
