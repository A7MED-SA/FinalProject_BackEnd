using Athary.Application.DTOs.Wishlist;

namespace Athary.Application.Interfaces.Wishlist;

public interface IWishlistService
{
    Task<WishlistResponseDto> GetWishlistAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<WishlistItemDto> AddItemAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default);
    Task RemoveItemAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default);
}
