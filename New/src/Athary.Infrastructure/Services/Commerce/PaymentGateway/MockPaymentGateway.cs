using Athary.Application.Interfaces.Commerce;

namespace Athary.Infrastructure.Services.Commerce.PaymentGateway;

public sealed class MockPaymentGateway : IPaymentGateway
{
    private static long _orderCounter;

    public Task<PaymentResult> ProcessPaymentAsync(
        decimal amount,
        string currency,
        Guid paymentMethodId,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var refNo = $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Interlocked.Increment(ref _orderCounter):D6}";
        return Task.FromResult(PaymentResult.Succeeded(refNo));
    }
}
