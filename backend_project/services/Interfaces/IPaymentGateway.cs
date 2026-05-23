namespace backend_project.Services.Interfaces;

public enum PaymentResultStatus
{
    Succeeded,
    Failed
}

public class PaymentResult
{
    public PaymentResultStatus Status { get; set; }
    public string TransactionRef { get; set; } = string.Empty;
    public string? GatewayMessage { get; set; }
    public DateTime ProcessedAt { get; set; }

    public static PaymentResult Succeeded(string transactionRef)
    {
        return new PaymentResult
        {
            Status = PaymentResultStatus.Succeeded,
            TransactionRef = transactionRef,
            ProcessedAt = DateTime.UtcNow,
            GatewayMessage = "Payment processed successfully"
        };
    }

    public static PaymentResult Failed(string message)
    {
        return new PaymentResult
        {
            Status = PaymentResultStatus.Failed,
            TransactionRef = string.Empty,
            GatewayMessage = message,
            ProcessedAt = DateTime.UtcNow
        };
    }
}

public interface IPaymentGateway
{
    Task<PaymentResult> ProcessPaymentAsync(
        decimal amount,
        string currency,
        Guid paymentMethodId,
        Dictionary<string, string>? metadata = null);
}
