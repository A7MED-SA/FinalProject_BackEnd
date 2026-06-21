using Athary.Application.DTOs.Commerce;

namespace Athary.Application.Interfaces.Commerce;

public interface ICartService
{
    Task<CartResponseDto> GetCartAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CartItemDto> AddItemAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default);
    Task RemoveItemAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default);
    Task<ApplyCouponResponse> ApplyCouponAsync(Guid userId, string code, CancellationToken cancellationToken = default);
    Task RemoveCouponAsync(Guid userId, CancellationToken cancellationToken = default);
}
