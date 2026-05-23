using backend_project.DTOs.Coupon;

namespace backend_project.Services.Interfaces;

public interface ICouponService
{
    Task<CouponResponseDto> CreateCouponAsync(CreateCouponDto dto, Guid createdBy);
    Task<CouponResponseDto?> GetCouponByIdAsync(Guid couponId);
    Task<IEnumerable<CouponResponseDto>> GetAllCouponsAsync(bool? isActive = null);
    Task<CouponResponseDto?> UpdateCouponAsync(Guid couponId, CreateCouponDto dto);
    Task<bool> ToggleCouponAsync(Guid couponId);
    Task<bool> DeleteCouponAsync(Guid couponId);
    Task<ValidateCouponResponse> ValidateCouponAsync(string code, decimal cartTotal, List<Guid> courseIds);
    Task<CouponValidationResult> ValidateAndApplyAsync(string code, decimal cartTotal, List<Guid> courseIds, Guid userId);
}
