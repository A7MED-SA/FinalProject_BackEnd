using Athary.Application.DTOs.Commerce;
using Athary.Application.Interfaces.Commerce;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Domain.Interfaces;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.Commerce;

public sealed class CouponService : ICouponService
{
    private readonly IRepository<Coupon> _couponRepo;
    private readonly IRepository<CouponUsage> _usageRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public CouponService(
        IRepository<Coupon> couponRepo,
        IRepository<CouponUsage> usageRepo,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
    {
        _couponRepo = couponRepo;
        _usageRepo = usageRepo;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<CouponResponseDto> CreateCouponAsync(CreateCouponDto dto, Guid createdBy, CancellationToken cancellationToken = default)
    {
        var exists = await _couponRepo.AnyAsync(c => c.Code == dto.Code, cancellationToken);
        if (exists)
            throw new InvalidOperationException("رمز الكوبون موجود بالفعل.");

        if (!Enum.TryParse<CouponType>(dto.Type, true, out var couponType))
            throw new InvalidOperationException("نوع الكوبون غير صالح.");

        if (!Enum.TryParse<CouponApplicableTo>(dto.ApplicableTo, true, out var applicableTo))
            throw new InvalidOperationException("نطاق تطبيق الكوبون غير صالح.");

        var coupon = new Coupon
        {
            Code = dto.Code,
            Type = couponType,
            Value = dto.Value,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            MinimumPurchaseAmount = dto.MinimumPurchaseAmount,
            ApplicableTo = applicableTo,
            UsageLimit = dto.UsageLimit,
            UserLimitPerUser = dto.UserLimitPerUser,
            IsPublic = dto.IsPublic,
            ValidFrom = dto.ValidFrom,
            ValidUntil = dto.ValidUntil,
            CreatedBy = createdBy,
        };

        await _couponRepo.AddAsync(coupon, cancellationToken);

        if (dto.CourseIds is { Count: > 0 } && applicableTo == CouponApplicableTo.SpecificCourses)
        {
            foreach (var courseId in dto.CourseIds)
            {
                coupon.CouponCourses.Add(new CouponCourse
                {
                    CouponId = coupon.Id,
                    CourseId = courseId
                });
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(coupon);
    }

    public async Task<CouponResponseDto?> GetCouponByIdAsync(Guid couponId, CancellationToken cancellationToken = default)
    {
        var coupon = await _couponRepo.GetByIdAsync(couponId, cancellationToken);
        return coupon is null ? null : MapToDto(coupon);
    }

    public async Task<IEnumerable<CouponResponseDto>> GetAllCouponsAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Coupons.AsNoTracking();

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        var coupons = await query.OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);
        return coupons.Select(MapToDto);
    }

    public async Task<CouponResponseDto?> UpdateCouponAsync(Guid couponId, CreateCouponDto dto, CancellationToken cancellationToken = default)
    {
        var coupon = await _couponRepo.GetByIdAsync(couponId, cancellationToken);
        if (coupon is null)
            return null;

        if (!Enum.TryParse<CouponType>(dto.Type, true, out var couponType))
            throw new InvalidOperationException("نوع الكوبون غير صالح.");

        if (!Enum.TryParse<CouponApplicableTo>(dto.ApplicableTo, true, out var applicableTo))
            throw new InvalidOperationException("نطاق تطبيق الكوبون غير صالح.");

        coupon.Code = dto.Code;
        coupon.Type = couponType;
        coupon.Value = dto.Value;
        coupon.MaxDiscountAmount = dto.MaxDiscountAmount;
        coupon.MinimumPurchaseAmount = dto.MinimumPurchaseAmount;
        coupon.ApplicableTo = applicableTo;
        coupon.UsageLimit = dto.UsageLimit;
        coupon.UserLimitPerUser = dto.UserLimitPerUser;
        coupon.IsPublic = dto.IsPublic;
        coupon.ValidFrom = dto.ValidFrom;
        coupon.ValidUntil = dto.ValidUntil;

        if (dto.CourseIds is { Count: > 0 } && applicableTo == CouponApplicableTo.SpecificCourses)
        {
            var existing = await _context.CouponCourses
                .Where(cc => cc.CouponId == couponId)
                .ToListAsync(cancellationToken);

            _context.CouponCourses.RemoveRange(existing);

            foreach (var courseId in dto.CourseIds)
            {
                _context.CouponCourses.Add(new CouponCourse
                {
                    CouponId = couponId,
                    CourseId = courseId
                });
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(coupon);
    }

    public async Task<bool> ToggleCouponAsync(Guid couponId, CancellationToken cancellationToken = default)
    {
        var coupon = await _couponRepo.GetByIdAsync(couponId, cancellationToken);
        if (coupon is null)
            return false;

        coupon.IsActive = !coupon.IsActive;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteCouponAsync(Guid couponId, CancellationToken cancellationToken = default)
    {
        var coupon = await _couponRepo.GetByIdAsync(couponId, cancellationToken);
        if (coupon is null)
            return false;

        await _couponRepo.DeleteAsync(coupon, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ValidateCouponResponse> ValidateCouponAsync(string code, decimal cartTotal, List<Guid> courseIds, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons
            .AsNoTracking()
            .Include(c => c.CouponCourses)
            .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);

        if (coupon is null)
            return new ValidateCouponResponse { Code = code, IsValid = false, Message = "الكوبون غير موجود." };

        var result = CalculateDiscount(coupon, cartTotal, courseIds, null);
        return new ValidateCouponResponse
        {
            Code = code,
            IsValid = result.IsValid,
            DiscountAmount = result.DiscountAmount,
            FinalAmount = result.FinalAmount,
            Message = result.Message
        };
    }

    public async Task<CouponValidationResult> ValidateAndApplyAsync(string code, decimal cartTotal, List<Guid> courseIds, Guid userId, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons
            .Include(c => c.CouponCourses)
            .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);

        if (coupon is null)
            return new CouponValidationResult { IsValid = false, Message = "الكوبون غير موجود." };

        var result = CalculateDiscount(coupon, cartTotal, courseIds, userId);

        if (result.IsValid)
        {
            coupon.TimesUsed++;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private CouponValidationResult CalculateDiscount(Coupon coupon, decimal cartTotal, List<Guid> courseIds, Guid? userId)
    {
        if (!coupon.IsActive)
            return new CouponValidationResult { IsValid = false, Message = "الكوبون غير نشط." };

        var now = DateTime.UtcNow;

        if (coupon.ValidFrom.HasValue && now < coupon.ValidFrom.Value)
            return new CouponValidationResult { IsValid = false, Message = "الكوبون غير متاح بعد." };

        if (coupon.ValidUntil.HasValue && now > coupon.ValidUntil.Value)
            return new CouponValidationResult { IsValid = false, Message = "انتهت صلاحية الكوبون." };

        if (coupon.MinimumPurchaseAmount.HasValue && cartTotal < coupon.MinimumPurchaseAmount.Value)
            return new CouponValidationResult { IsValid = false, Message = $"الحد الأدنى للشراء هو {coupon.MinimumPurchaseAmount.Value}." };

        if (coupon.UsageLimit.HasValue && coupon.TimesUsed >= coupon.UsageLimit.Value)
            return new CouponValidationResult { IsValid = false, Message = "تم استنفاذ عدد استخدامات الكوبون." };

        if (coupon.ApplicableTo == CouponApplicableTo.SpecificCourses && courseIds.Count > 0)
        {
            var applicableCourseIds = coupon.CouponCourses.Select(cc => cc.CourseId).ToHashSet();
            var matching = courseIds.Count(cid => applicableCourseIds.Contains(cid));
            if (matching == 0)
                return new CouponValidationResult { IsValid = false, Message = "الكوبون غير صالح لهذه الدورات." };
        }

        if (userId.HasValue)
        {
            var usageCount = _context.CouponUsages
                .Count(cu => cu.CouponId == coupon.Id && cu.UserId == userId.Value);

            if (coupon.UserLimitPerUser.HasValue && usageCount >= coupon.UserLimitPerUser.Value)
                return new CouponValidationResult { IsValid = false, Message = "لقد استخدمت هذا الكوبون بالفعل." };
        }

        var discount = coupon.Type switch
        {
            CouponType.Percentage => coupon.Value / 100m * cartTotal,
            CouponType.Fixed => coupon.Value,
            _ => 0
        };

        if (coupon.MaxDiscountAmount.HasValue && discount > coupon.MaxDiscountAmount.Value)
            discount = coupon.MaxDiscountAmount.Value;

        var final = Math.Max(0, cartTotal - discount);

        return new CouponValidationResult
        {
            IsValid = true,
            DiscountAmount = discount,
            FinalAmount = final
        };
    }

    private static CouponResponseDto MapToDto(Coupon coupon) => new()
    {
        Id = coupon.Id,
        Code = coupon.Code,
        Type = coupon.Type.ToString(),
        Value = coupon.Value,
        MaxDiscountAmount = coupon.MaxDiscountAmount,
        MinimumPurchaseAmount = coupon.MinimumPurchaseAmount,
        ApplicableTo = coupon.ApplicableTo.ToString(),
        UsageLimit = coupon.UsageLimit,
        UserLimitPerUser = coupon.UserLimitPerUser,
        TimesUsed = coupon.TimesUsed,
        IsActive = coupon.IsActive,
        ValidFrom = coupon.ValidFrom,
        ValidUntil = coupon.ValidUntil,
        CreatedAt = coupon.CreatedAt
    };
}
