using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations.PaymentGateway;

public class MockPaymentGateway : IPaymentGateway
{
    private static int _transactionCounter = 0;

    private static readonly HashSet<Guid> _successMethodIds = new()
    {
        Guid.Parse("00000000-0000-0000-0000-000000000001"),
        Guid.Parse("00000000-0000-0000-0000-000000000002"),
    };

    private static readonly HashSet<Guid> _failMethodIds = new()
    {
        Guid.Parse("00000000-0000-0000-0000-000000000000"),
    };

    public async Task<PaymentResult> ProcessPaymentAsync(
        decimal amount,
        string currency,
        Guid paymentMethodId,
        Dictionary<string, string>? metadata = null)
    {
        await Task.Delay(100);

        var counter = Interlocked.Increment(ref _transactionCounter);

        if (_failMethodIds.Contains(paymentMethodId))
        {
            return PaymentResult.Failed("Transaction declined by mock gateway");
        }

        if (_successMethodIds.Contains(paymentMethodId))
        {
            var txnRef = $"TXN-{DateTime.UtcNow:yyyyMMdd}-{counter:D6}";
            return PaymentResult.Succeeded(txnRef);
        }

        return PaymentResult.Succeeded($"TXN-{DateTime.UtcNow:yyyyMMdd}-{counter:D6}");
    }
}
