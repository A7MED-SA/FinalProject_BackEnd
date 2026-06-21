namespace Athary.Application.Interfaces.Commerce;

public enum PaymentResultStatus
{
    Succeeded,
    Failed
}

public sealed record PaymentResult
{
    public PaymentResultStatus Status { get; init; }
    public string TransactionRef { get; init; } = string.Empty;
    public string? GatewayMessage { get; init; }
    public DateTime ProcessedAt { get; init; }

    public static PaymentResult Succeeded(string transactionRef) => new()
    {
        Status = PaymentResultStatus.Succeeded,
        TransactionRef = transactionRef,
        ProcessedAt = DateTime.UtcNow,
        GatewayMessage = "Payment processed successfully"
    };

    public static PaymentResult Failed(string message) => new()
    {
        Status = PaymentResultStatus.Failed,
        TransactionRef = string.Empty,
        GatewayMessage = message,
        ProcessedAt = DateTime.UtcNow
    };
}

public interface IPaymentGateway
{
    Task<PaymentResult> ProcessPaymentAsync(
        decimal amount,
        string currency,
        Guid paymentMethodId,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies webhook signature from payment provider.
    /// Override this for production (Stripe HMAC, PayPal webhook verify, etc.).
    /// </summary>
    Task<bool> VerifyWebhookSignatureAsync(string payload, string signatureHeader, CancellationToken cancellationToken = default)
        => Task.FromResult(true);
}
