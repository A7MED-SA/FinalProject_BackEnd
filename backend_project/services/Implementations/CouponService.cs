using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.Coupon;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class CouponService : ICouponService
{
    private readonly ApplicationDbContext _context;

    public CouponService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CouponResponseDto> CreateCouponAsync(CreateCouponDto dto, Guid createdBy)
    {
        var existing = await _context.Coupons.AnyAsync(c => c.Code == dto.Code);
        if (existing)
            throw new InvalidOperationException("A coupon with this code already exists.");

        var coupon = new Coupon
        {
            Code = dto.Code.ToUpper(),
            Type = dto.Type == "Percentage" ? CouponType.Percentage : CouponType.Fixed,
            Value = dto.Value,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            MinimumPurchaseAmount = dto.MinimumPurchaseAmount,
            ApplicableTo = dto.ApplicableTo switch
            {
                "SpecificCourses" => CouponApplicableTo.SpecificCourses,
                "Category" => CouponApplicableTo.Category,
                _ => CouponApplicableTo.All
            },
            UsageLimit = dto.UsageLimit,
            UserLimitPerUser = dto.UserLimitPerUser,
            IsPublic = dto.IsPublic,
            ValidFrom = dto.ValidFrom,
            ValidUntil = dto.ValidUntil,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        if (dto.CourseIds?.Any() == true && dto.ApplicableTo == "SpecificCourses")
        {
            foreach (var courseId in dto.CourseIds)
            {
                _context.CouponCourses.Add(new CouponCourse
                {
                    CouponId = coupon.Id,
                    CourseId = courseId
                });
            }
            await _context.SaveChangesAsync();
        }

        return MapToDto(coupon);
    }

    public async Task<CouponResponseDto?> GetCouponByIdAsync(Guid couponId)
    {
        var coupon = await _context.Coupons.FindAsync(couponId);
        return coupon == null ? null : MapToDto(coupon);
    }

    public async Task<IEnumerable<CouponResponseDto>> GetAllCouponsAsync(bool? isActive = null)
    {
        var query = _context.Coupons.AsQueryable();
        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        var coupons = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return coupons.Select(MapToDto);
    }

    public async Task<CouponResponseDto?> UpdateCouponAsync(Guid couponId, CreateCouponDto dto)
    {
        var coupon = await _context.Coupons.FindAsync(couponId);
        if (coupon == null) return null;

        var codeTaken = await _context.Coupons.AnyAsync(c => c.Code == dto.Code && c.Id != couponId);
        if (codeTaken)
            throw new InvalidOperationException("A coupon with this code already exists.");

        coupon.Code = dto.Code.ToUpper();
        coupon.Type = dto.Type == "Percentage" ? CouponType.Percentage : CouponType.Fixed;
        coupon.Value = dto.Value;
        coupon.MaxDiscountAmount = dto.MaxDiscountAmount;
        coupon.MinimumPurchaseAmount = dto.MinimumPurchaseAmount;
        coupon.ApplicableTo = dto.ApplicableTo switch
        {
            "SpecificCourses" => CouponApplicableTo.SpecificCourses,
            "Category" => CouponApplicableTo.Category,
            _ => CouponApplicableTo.All
        };
        coupon.UsageLimit = dto.UsageLimit;
        coupon.UserLimitPerUser = dto.UserLimitPerUser;
        coupon.IsPublic = dto.IsPublic;
        coupon.ValidFrom = dto.ValidFrom;
        coupon.ValidUntil = dto.ValidUntil;

        await _context.SaveChangesAsync();
        return MapToDto(coupon);
    }

    public async Task<bool> ToggleCouponAsync(Guid couponId)
    {
        var coupon = await _context.Coupons.FindAsync(couponId);
        if (coupon == null) return false;

        coupon.IsActive = !coupon.IsActive;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCouponAsync(Guid couponId)
    {
        var coupon = await _context.Coupons.FindAsync(couponId);
        if (coupon == null) return false;

        _context.Coupons.Remove(coupon);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ValidateCouponResponse> ValidateCouponAsync(string code, decimal cartTotal, List<Guid> courseIds)
    {
        var coupon = await _context.Coupons
            .Include(c => c.CouponCourses)
            .FirstOrDefaultAsync(c => c.Code == code.ToUpper());

        if (coupon == null)
            return InvalidResponse("Invalid coupon code.");

        if (!coupon.IsActive)
            return InvalidResponse("This coupon is no longer active.");

        if (coupon.ValidFrom.HasValue && DateTime.UtcNow < coupon.ValidFrom.Value)
            return InvalidResponse("This coupon is not yet valid.");

        if (coupon.ValidUntil.HasValue && DateTime.UtcNow > coupon.ValidUntil.Value)
            return InvalidResponse("This coupon has expired.");

        if (coupon.UsageLimit.HasValue && coupon.TimesUsed >= coupon.UsageLimit.Value)
            return InvalidResponse("This coupon has reached its usage limit.");

        if (coupon.MinimumPurchaseAmount.HasValue && cartTotal < coupon.MinimumPurchaseAmount.Value)
            return InvalidResponse($"Minimum purchase amount of {coupon.MinimumPurchaseAmount.Value} not met.");

        if (coupon.ApplicableTo == CouponApplicableTo.SpecificCourses && courseIds.Any())
        {
            var validCourseIds = coupon.CouponCourses.Select(cc => cc.CourseId).ToHashSet();
            if (!courseIds.Any(cid => validCourseIds.Contains(cid)))
                return InvalidResponse("This coupon is not applicable to the selected courses.");
        }

        var discount = CalculateDiscount(coupon, cartTotal);
        return new ValidateCouponResponse
        {
            Code = coupon.Code,
            IsValid = true,
            DiscountAmount = discount,
            FinalAmount = cartTotal - discount,
            Message = "Coupon applied successfully."
        };
    }

    public async Task<CouponValidationResult> ValidateAndApplyAsync(string code, decimal cartTotal, List<Guid> courseIds, Guid userId)
    {
        var validation = await ValidateCouponAsync(code, cartTotal, courseIds);
        if (!validation.IsValid)
            return new CouponValidationResult
            {
                IsValid = false,
                Message = validation.Message
            };

        var coupon = await _context.Coupons.FirstAsync(c => c.Code == code.ToUpper());

        if (coupon.UserLimitPerUser.HasValue)
        {
            var usageCount = await _context.CouponUsages
                .CountAsync(cu => cu.CouponId == coupon.Id && cu.UserId == userId);
            if (usageCount >= coupon.UserLimitPerUser.Value)
                return new CouponValidationResult
                {
                    IsValid = false,
                    Message = "You have already used this coupon the maximum number of times."
                };
        }

        return new CouponValidationResult
        {
            IsValid = true,
            DiscountAmount = validation.DiscountAmount,
            FinalAmount = validation.FinalAmount
        };
    }

    private decimal CalculateDiscount(Coupon coupon, decimal cartTotal)
    {
        if (coupon.Type == CouponType.Percentage)
        {
            var discount = cartTotal * (coupon.Value / 100);
            if (coupon.MaxDiscountAmount.HasValue && discount > coupon.MaxDiscountAmount.Value)
                discount = coupon.MaxDiscountAmount.Value;
            return Math.Round(discount, 2);
        }

        return Math.Min(coupon.Value, cartTotal);
    }

    private static ValidateCouponResponse InvalidResponse(string message)
    {
        return new ValidateCouponResponse
        {
            IsValid = false,
            Message = message,
            DiscountAmount = 0,
            FinalAmount = 0
        };
    }

    private static CouponResponseDto MapToDto(Coupon c)
    {
        return new CouponResponseDto
        {
            Id = c.Id,
            Code = c.Code,
            Type = c.Type.ToString(),
            Value = c.Value,
            MaxDiscountAmount = c.MaxDiscountAmount,
            MinimumPurchaseAmount = c.MinimumPurchaseAmount,
            ApplicableTo = c.ApplicableTo.ToString(),
            UsageLimit = c.UsageLimit,
            UserLimitPerUser = c.UserLimitPerUser,
            TimesUsed = c.TimesUsed,
            IsActive = c.IsActive,
            ValidFrom = c.ValidFrom,
            ValidUntil = c.ValidUntil,
            CreatedAt = c.CreatedAt
        };
    }
}
