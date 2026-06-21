namespace Athary.Application.DTOs.Commerce;

// ─── Cart ───────────────────────────────────────────────────────────

public sealed record CartResponseDto
{
    public Guid Id { get; init; }
    public List<CartItemDto> Items { get; init; } = new();
    public decimal Subtotal { get; init; }
    public string? CouponCode { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal FinalAmount { get; init; }
}

public sealed record CartItemDto
{
    public Guid Id { get; init; }
    public Guid CourseId { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public string? CourseImageUrl { get; init; }
    public string? InstructorName { get; init; }
    public decimal PriceSnapshot { get; init; }
    public decimal CurrentPrice { get; init; }
    public DateTime AddedAt { get; init; }
}

public sealed record AddToCartRequest
{
    public Guid CourseId { get; init; }
}

public sealed record ApplyCouponRequest
{
    public string Code { get; init; } = string.Empty;
}

public sealed record ApplyCouponResponse
{
    public string Code { get; init; } = string.Empty;
    public decimal DiscountAmount { get; init; }
    public decimal FinalAmount { get; init; }
    public string? Message { get; init; }
}

// ─── Order ──────────────────────────────────────────────────────────

public sealed record CreateOrderRequest
{
    public string? CouponCode { get; init; }
}

public sealed record OrderResponseDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public decimal Subtotal { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal FinalAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? CouponCode { get; init; }
    public int ItemCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed record OrderDetailDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public decimal Subtotal { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal FinalAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? CouponCode { get; init; }
    public List<OrderItemDto> Items { get; init; } = new();
    public List<PaymentHistoryDto> Payments { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}

public sealed record OrderItemDto
{
    public Guid Id { get; init; }
    public Guid CourseId { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public decimal PriceAtPurchase { get; init; }
}

public sealed record PaymentHistoryDto
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? GatewayResponse { get; init; }
    public DateTime CreatedAt { get; init; }
}

// ─── Payment ────────────────────────────────────────────────────────

public sealed record ProcessPaymentRequest
{
    public Guid PaymentMethodId { get; init; }
}

public sealed record PaymentResponseDto
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? GatewayTransactionId { get; init; }
    public string? GatewayResponse { get; init; }
    public string? PaymentMethodName { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed record PaymentMethodResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string? Configuration { get; init; }
}

public sealed record CreatePaymentMethodRequest
{
    public string Name { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string Type { get; init; } = "CreditCard";
    public string? Configuration { get; init; }
}

// ─── Coupon ─────────────────────────────────────────────────────────

public sealed record CreateCouponDto
{
    public string Code { get; init; } = string.Empty;
    public string Type { get; init; } = "Percentage";
    public decimal Value { get; init; }
    public decimal? MaxDiscountAmount { get; init; }
    public decimal? MinimumPurchaseAmount { get; init; }
    public string ApplicableTo { get; init; } = "All";
    public List<Guid>? CourseIds { get; init; }
    public int? UsageLimit { get; init; }
    public int? UserLimitPerUser { get; init; }
    public bool IsPublic { get; init; } = true;
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidUntil { get; init; }
}

public sealed record CouponResponseDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public decimal Value { get; init; }
    public decimal? MaxDiscountAmount { get; init; }
    public decimal? MinimumPurchaseAmount { get; init; }
    public string ApplicableTo { get; init; } = string.Empty;
    public int? UsageLimit { get; init; }
    public int? UserLimitPerUser { get; init; }
    public int TimesUsed { get; init; }
    public bool IsActive { get; init; }
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidUntil { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed record ValidateCouponRequest
{
    public string Code { get; init; } = string.Empty;
    public decimal CartTotal { get; init; }
    public List<Guid> CourseIds { get; init; } = new();
}

public sealed record ValidateCouponResponse
{
    public string Code { get; init; } = string.Empty;
    public bool IsValid { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal FinalAmount { get; init; }
    public string? Message { get; init; }
}

public sealed record CouponValidationResult
{
    public decimal DiscountAmount { get; init; }
    public decimal FinalAmount { get; init; }
    public bool IsValid { get; init; }
    public string? Message { get; init; }
}

// ─── Refund ─────────────────────────────────────────────────────────

public sealed record RequestRefundRequest
{
    public Guid PaymentId { get; init; }
    public string? Reason { get; init; }
}

public sealed record ProcessRefundRequest
{
    public Guid RefundId { get; init; }
    public string? AdminNotes { get; init; }
}

public sealed record RefundResponseDto
{
    public Guid Id { get; init; }
    public Guid PaymentId { get; init; }
    public decimal Amount { get; init; }
    public string? Reason { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? OrderNumber { get; init; }
    public DateTime RequestedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public string? ProcessedByName { get; init; }
}
