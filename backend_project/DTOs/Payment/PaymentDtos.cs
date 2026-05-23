namespace backend_project.DTOs.Payment;

public class ProcessPaymentRequest
{
    public Guid PaymentMethodId { get; set; }
}

public class PaymentResponseDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? GatewayTransactionId { get; set; }
    public string? GatewayResponse { get; set; }
    public int AttemptNumber { get; set; }
    public string? PaymentMethodName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaymentMethodResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}

public class CreatePaymentMethodRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public int SortOrder { get; set; }
}
