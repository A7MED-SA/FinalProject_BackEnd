using Athary.Application.DTOs.Commerce;

namespace Athary.Application.Interfaces.Commerce;

public interface IPaymentService
{
    Task<PaymentResponseDto> ProcessPaymentAsync(Guid userId, Guid orderId, Guid paymentMethodId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentResponseDto>> GetPaymentHistoryAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<PaymentMethodResponse> CreatePaymentMethodAsync(Guid adminUserId, CreatePaymentMethodRequest request, CancellationToken cancellationToken = default);
    Task<bool> TogglePaymentMethodAsync(Guid paymentMethodId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentMethodResponse>> GetActivePaymentMethodsAsync(CancellationToken cancellationToken = default);
}
