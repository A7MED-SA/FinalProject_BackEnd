namespace backend_project.DTOs.Cart;

public class CartResponseDto
{
    public Guid Id { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public string? CouponCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
}

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseImageUrl { get; set; }
    public string? InstructorName { get; set; }
    public decimal PriceSnapshot { get; set; }
    public decimal CurrentPrice { get; set; }
    public DateTime AddedAt { get; set; }
}

public class AddToCartRequest
{
    public Guid CourseId { get; set; }
}

public class ApplyCouponRequest
{
    public string Code { get; set; } = string.Empty;
}

public class ApplyCouponResponse
{
    public string Code { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
}
