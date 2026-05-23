using backend_project.DTOs.Wishlist;

namespace backend_project.Services.Interfaces;

public interface IWishlistService
{
    Task<WishlistResponseDto> GetWishlistAsync(Guid userId);
    Task<WishlistItemDto> AddItemAsync(Guid userId, Guid courseId);
    Task RemoveItemAsync(Guid userId, Guid courseId);
}
