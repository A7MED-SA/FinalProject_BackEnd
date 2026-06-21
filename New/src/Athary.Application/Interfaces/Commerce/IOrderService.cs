using Athary.Application.DTOs.Commerce;

namespace Athary.Application.Interfaces.Commerce;

public interface IOrderService
{
    Task<OrderResponseDto> CreateOrderAsync(Guid userId, CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderResponseDto>> GetOrdersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<OrderDetailDto> GetOrderDetailsAsync(Guid userId, Guid orderId, CancellationToken cancellationToken = default);
}
