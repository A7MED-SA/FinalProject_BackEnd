using backend_project.DTOs.Payment;

namespace backend_project.Services.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponseDto> ProcessPaymentAsync(Guid userId, Guid orderId, Guid paymentMethodId);
    Task<IEnumerable<PaymentResponseDto>> GetPaymentHistoryAsync(Guid orderId);
    Task<PaymentMethodResponse> CreatePaymentMethodAsync(Guid adminUserId, CreatePaymentMethodRequest request);
    Task<bool> TogglePaymentMethodAsync(Guid paymentMethodId);
    Task<IEnumerable<PaymentMethodResponse>> GetActivePaymentMethodsAsync();
}
