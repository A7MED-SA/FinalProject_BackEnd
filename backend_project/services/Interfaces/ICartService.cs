using backend_project.DTOs.Cart;

namespace backend_project.Services.Interfaces;

public interface ICartService
{
    Task<CartResponseDto> GetCartAsync(Guid userId);
    Task<CartItemDto> AddItemAsync(Guid userId, Guid courseId);
    Task RemoveItemAsync(Guid userId, Guid itemId);
    Task<ApplyCouponResponse> ApplyCouponAsync(Guid userId, string code);
    Task RemoveCouponAsync(Guid userId);
}
