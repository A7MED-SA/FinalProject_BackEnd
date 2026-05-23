namespace backend_project.DTOs.Refund;

public class RequestRefundRequest
{
    public Guid PaymentId { get; set; }
    public string? Reason { get; set; }
}

public class ProcessRefundRequest
{
    public Guid RefundId { get; set; }
    public string? AdminNotes { get; set; }
}

public class RefundResponseDto
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? OrderNumber { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedByName { get; set; }
}
