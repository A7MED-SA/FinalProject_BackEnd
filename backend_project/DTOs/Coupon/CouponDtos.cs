namespace backend_project.DTOs.Coupon;

public class CreateCouponDto
{
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = "Percentage";
    public decimal Value { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinimumPurchaseAmount { get; set; }
    public string ApplicableTo { get; set; } = "All";
    public List<Guid>? CourseIds { get; set; }
    public int? UsageLimit { get; set; }
    public int? UserLimitPerUser { get; set; }
    public bool IsPublic { get; set; } = true;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
}

public class CouponResponseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinimumPurchaseAmount { get; set; }
    public string ApplicableTo { get; set; } = string.Empty;
    public int? UsageLimit { get; set; }
    public int? UserLimitPerUser { get; set; }
    public int TimesUsed { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ValidateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public decimal CartTotal { get; set; }
    public List<Guid> CourseIds { get; set; } = new();
}

public class ValidateCouponResponse
{
    public string Code { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string? Message { get; set; }
}

public class CouponValidationResult
{
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public bool IsValid { get; set; }
    public string? Message { get; set; }
}
