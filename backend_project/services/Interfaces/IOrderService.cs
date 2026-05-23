using backend_project.DTOs.Order;

namespace backend_project.Services.Interfaces;

public interface IOrderService
{
    Task<OrderResponseDto> CreateOrderAsync(Guid userId, CreateOrderRequest request);
    Task<IEnumerable<OrderResponseDto>> GetOrdersAsync(Guid userId);
    Task<OrderDetailDto> GetOrderDetailsAsync(Guid userId, Guid orderId);
}
