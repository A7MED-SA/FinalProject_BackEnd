using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.Coupon;
using backend_project.Models;
using backend_project.Services.Implementations;
using CommerceTests.Helpers;

namespace CommerceTests;

public class CouponServiceTests
{
    [Fact]
    public async Task CreateCoupon_Percentage_CreatesSuccessfully()
    {
        using var context = TestDbContextFactory.Create();
        var service = new CouponService(context);
        var adminId = Guid.NewGuid();

        var dto = new CreateCouponDto
        {
            Code = "SAVE10",
            Type = "Percentage",
            Value = 10,
            MaxDiscountAmount = 50,
            ApplicableTo = "All",
            IsPublic = true
        };

        var result = await service.CreateCouponAsync(dto, adminId);
        result.Code.Should().Be("SAVE10");
        result.Value.Should().Be(10);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateCoupon_ValidCode_ReturnsSuccess()
    {
        using var context = TestDbContextFactory.Create();
        var service = new CouponService(context);
        var adminId = Guid.NewGuid();

        await service.CreateCouponAsync(new CreateCouponDto
        {
            Code = "WELCOME20",
            Type = "Percentage",
            Value = 20,
            MaxDiscountAmount = 100,
            ApplicableTo = "All",
            IsPublic = true,
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidUntil = DateTime.UtcNow.AddDays(30)
        }, adminId);

        var result = await service.ValidateCouponAsync("WELCOME20", 500, new List<Guid>());
        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(100);
        result.FinalAmount.Should().Be(400);
    }

    [Fact]
    public async Task ValidateCoupon_Expired_ReturnsFailure()
    {
        using var context = TestDbContextFactory.Create();
        var service = new CouponService(context);
        var adminId = Guid.NewGuid();

        await service.CreateCouponAsync(new CreateCouponDto
        {
            Code = "EXPIRED",
            Type = "Percentage",
            Value = 10,
            ApplicableTo = "All",
            ValidUntil = DateTime.UtcNow.AddDays(-1)
        }, adminId);

        var result = await service.ValidateCouponAsync("EXPIRED", 500, new List<Guid>());
        result.IsValid.Should().BeFalse();
        result.Message.Should().Be("This coupon has expired.");
    }

    [Fact]
    public async Task ValidateCoupon_UsageLimitReached_ReturnsFailure()
    {
        using var context = TestDbContextFactory.Create();
        var service = new CouponService(context);
        var adminId = Guid.NewGuid();

        await service.CreateCouponAsync(new CreateCouponDto
        {
            Code = "LIMITED",
            Type = "Fixed",
            Value = 50,
            UsageLimit = 1,
            ApplicableTo = "All"
        }, adminId);

        var coupon = await context.Coupons.FirstAsync(c => c.Code == "LIMITED");
        coupon.TimesUsed = 1;
        await context.SaveChangesAsync();

        var result = await service.ValidateCouponAsync("LIMITED", 500, new List<Guid>());
        result.IsValid.Should().BeFalse();
        result.Message.Should().Be("This coupon has reached its usage limit.");
    }

    [Fact]
    public async Task ValidateCoupon_MinimumPurchaseNotMet_ReturnsFailure()
    {
        using var context = TestDbContextFactory.Create();
        var service = new CouponService(context);
        var adminId = Guid.NewGuid();

        await service.CreateCouponAsync(new CreateCouponDto
        {
            Code = "MIN100",
            Type = "Fixed",
            Value = 20,
            MinimumPurchaseAmount = 100,
            ApplicableTo = "All"
        }, adminId);

        var result = await service.ValidateCouponAsync("MIN100", 50, new List<Guid>());
        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("Minimum purchase amount");
    }

    [Fact]
    public async Task ValidateAndApply_ValidCoupon_RecordsUsage()
    {
        using var context = TestDbContextFactory.Create();
        var service = new CouponService(context);
        var adminId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await service.CreateCouponAsync(new CreateCouponDto
        {
            Code = "USER10",
            Type = "Percentage",
            Value = 10,
            UserLimitPerUser = 3,
            ApplicableTo = "All"
        }, adminId);

        var result = await service.ValidateAndApplyAsync("USER10", 200, new List<Guid>(), userId);
        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(20);
        result.FinalAmount.Should().Be(180);
    }

    [Fact]
    public async Task ValidateCoupon_SpecificCourses_NotApplicable_ReturnsFailure()
    {
        using var context = TestDbContextFactory.Create();
        var service = new CouponService(context);
        var adminId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        await service.CreateCouponAsync(new CreateCouponDto
        {
            Code = "SPECIFIC",
            Type = "Percentage",
            Value = 10,
            ApplicableTo = "SpecificCourses",
            CourseIds = new List<Guid> { courseId }
        }, adminId);

        var otherCourseId = Guid.NewGuid();
        var result = await service.ValidateCouponAsync("SPECIFIC", 200, new List<Guid> { otherCourseId });
        result.IsValid.Should().BeFalse();
        result.Message.Should().Be("This coupon is not applicable to the selected courses.");
    }
}
