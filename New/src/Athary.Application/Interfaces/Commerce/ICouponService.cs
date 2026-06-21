using Athary.Application.DTOs.Commerce;

namespace Athary.Application.Interfaces.Commerce;

public interface ICouponService
{
    Task<CouponResponseDto> CreateCouponAsync(CreateCouponDto dto, Guid createdBy, CancellationToken cancellationToken = default);
    Task<CouponResponseDto?> GetCouponByIdAsync(Guid couponId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CouponResponseDto>> GetAllCouponsAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task<CouponResponseDto?> UpdateCouponAsync(Guid couponId, CreateCouponDto dto, CancellationToken cancellationToken = default);
    Task<bool> ToggleCouponAsync(Guid couponId, CancellationToken cancellationToken = default);
    Task<bool> DeleteCouponAsync(Guid couponId, CancellationToken cancellationToken = default);
    Task<ValidateCouponResponse> ValidateCouponAsync(string code, decimal cartTotal, List<Guid> courseIds, CancellationToken cancellationToken = default);
    Task<CouponValidationResult> ValidateAndApplyAsync(string code, decimal cartTotal, List<Guid> courseIds, Guid userId, CancellationToken cancellationToken = default);
}
