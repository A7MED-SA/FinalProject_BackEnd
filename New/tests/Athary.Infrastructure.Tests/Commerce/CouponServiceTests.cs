using Athary.Application.DTOs.Commerce;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Repositories;
using Athary.Infrastructure.Services.Commerce;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Tests.Commerce;

public sealed class CouponServiceTests : SqliteTestBase
{
    private readonly CouponService _sut;
    private readonly User _admin;
    private readonly Course _course;

    public CouponServiceTests()
    {
        _admin = new User { FirstName = "Admin", LastName = "U", Email = "admin@t.com", UserName = "admin" };
        Context.Users.Add(_admin);

        var cat = new Category { Name = "Cat", Slug = "cat" };
        Context.Categories.Add(cat);
        Context.SaveChanges();

        _course = new Course
        {
            Title = "C", Slug = "c", Price = 100, CategoryId = cat.Id,
            CreatedBy = _admin.Id, Status = CourseStatus.Published, IsPublished = true
        };
        Context.Courses.Add(_course);
        Context.SaveChanges();

        _sut = new CouponService(
            new GenericRepository<Coupon>(Context),
            new GenericRepository<CouponUsage>(Context),
            new UnitOfWork(Context),
            Context);
    }

    [Fact]
    public async Task CreateCouponAsync_ShouldCreatePercentageCoupon()
    {
        var dto = new CreateCouponDto
        {
            Code = "SAVE20",
            Type = "Percentage",
            Value = 20
        };

        var result = await _sut.CreateCouponAsync(dto, _admin.Id);

        result.Code.Should().Be("SAVE20");
        result.Type.Should().Be("Percentage");
        result.Value.Should().Be(20);
        result.IsActive.Should().BeTrue();

        var saved = await Context.Coupons.FirstAsync(c => c.Code == "SAVE20");
        saved.Id.Should().Be(result.Id);
    }

    [Fact]
    public async Task CreateCouponAsync_ShouldCreateFixedCoupon()
    {
        var dto = new CreateCouponDto
        {
            Code = "FIXED50",
            Type = "Fixed",
            Value = 50,
            MinimumPurchaseAmount = 200,
            UsageLimit = 100
        };

        var result = await _sut.CreateCouponAsync(dto, _admin.Id);

        result.Type.Should().Be("Fixed");
        result.Value.Should().Be(50);
        result.MinimumPurchaseAmount.Should().Be(200);
        result.UsageLimit.Should().Be(100);
    }

    [Fact]
    public async Task CreateCouponAsync_ShouldThrow_WhenDuplicateCode()
    {
        var dto = new CreateCouponDto { Code = "DUPE", Type = "Percentage", Value = 10 };
        await _sut.CreateCouponAsync(dto, _admin.Id);

        await FluentActions.Invoking(() => _sut.CreateCouponAsync(dto, _admin.Id))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ToggleCouponAsync_ShouldToggleIsActive()
    {
        var dto = new CreateCouponDto { Code = "TOGGLE", Type = "Percentage", Value = 10 };
        var created = await _sut.CreateCouponAsync(dto, _admin.Id);
        created.IsActive.Should().BeTrue();

        var toggled = await _sut.ToggleCouponAsync(created.Id);
        toggled.Should().BeTrue();

        var after = await _sut.GetCouponByIdAsync(created.Id);
        after!.IsActive.Should().BeFalse();

        await _sut.ToggleCouponAsync(created.Id);
        after = await _sut.GetCouponByIdAsync(created.Id);
        after!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCouponAsync_ShouldRemoveCoupon()
    {
        var dto = new CreateCouponDto { Code = "DELETE", Type = "Percentage", Value = 10 };
        var created = await _sut.CreateCouponAsync(dto, _admin.Id);

        var deleted = await _sut.DeleteCouponAsync(created.Id);
        deleted.Should().BeTrue();

        var after = await _sut.GetCouponByIdAsync(created.Id);
        after.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCouponAsync_ShouldReturnFalse_WhenNotFound()
    {
        var result = await _sut.DeleteCouponAsync(Guid.NewGuid());
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateCouponAsync_ShouldReturnValid_ForActivePercentage()
    {
        await _sut.CreateCouponAsync(new CreateCouponDto { Code = "SAVE10", Type = "Percentage", Value = 10 }, _admin.Id);

        var result = await _sut.ValidateCouponAsync("SAVE10", 100, new List<Guid>());

        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(10);
        result.FinalAmount.Should().Be(90);
    }

    [Fact]
    public async Task ValidateCouponAsync_ShouldReturnInvalid_WhenExpired()
    {
        await _sut.CreateCouponAsync(new CreateCouponDto
        {
            Code = "EXPIRED", Type = "Percentage", Value = 10,
            ValidUntil = DateTime.UtcNow.AddDays(-1)
        }, _admin.Id);

        var result = await _sut.ValidateCouponAsync("EXPIRED", 100, new List<Guid>());

        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("انتهت");
    }

    [Fact]
    public async Task ValidateCouponAsync_ShouldReturnInvalid_WhenInactive()
    {
        var dto = new CreateCouponDto { Code = "INACTIVE", Type = "Percentage", Value = 10 };
        var created = await _sut.CreateCouponAsync(dto, _admin.Id);
        await _sut.ToggleCouponAsync(created.Id);

        var result = await _sut.ValidateCouponAsync("INACTIVE", 100, new List<Guid>());

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateCouponAsync_ShouldReturnInvalid_WhenBelowMinPurchase()
    {
        await _sut.CreateCouponAsync(new CreateCouponDto
        {
            Code = "MIN200", Type = "Fixed", Value = 30,
            MinimumPurchaseAmount = 200
        }, _admin.Id);

        var result = await _sut.ValidateCouponAsync("MIN200", 150, new List<Guid>());

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateCouponAsync_ShouldApplyMaxDiscount_ForPercentage()
    {
        await _sut.CreateCouponAsync(new CreateCouponDto
        {
            Code = "MAXCAP", Type = "Percentage", Value = 50,
            MaxDiscountAmount = 25
        }, _admin.Id);

        var result = await _sut.ValidateCouponAsync("MAXCAP", 100, new List<Guid>());

        result.DiscountAmount.Should().Be(25);
        result.FinalAmount.Should().Be(75);
    }

    [Fact]
    public async Task ValidateCouponAsync_ShouldRespectSpecificCourses()
    {
        var course2 = new Course
        {
            Title = "C2", Slug = "c2", Price = 100, CategoryId = Context.Categories.First().Id,
            CreatedBy = _admin.Id, Status = CourseStatus.Published, IsPublished = true
        };
        Context.Courses.Add(course2);
        await Context.SaveChangesAsync();

        await _sut.CreateCouponAsync(new CreateCouponDto
        {
            Code = "SPECIFIC", Type = "Percentage", Value = 10,
            ApplicableTo = "SpecificCourses",
            CourseIds = new List<Guid> { _course.Id }
        }, _admin.Id);

        var resultValid = await _sut.ValidateCouponAsync("SPECIFIC", 100, new List<Guid> { _course.Id });
        resultValid.IsValid.Should().BeTrue();

        var resultInvalid = await _sut.ValidateCouponAsync("SPECIFIC", 100, new List<Guid> { course2.Id });
        resultInvalid.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAndApplyAsync_ShouldIncrementUsage()
    {
        var dto = new CreateCouponDto
        {
            Code = "USAGE", Type = "Percentage", Value = 10,
            UsageLimit = 5
        };
        await _sut.CreateCouponAsync(dto, _admin.Id);

        var result = await _sut.ValidateAndApplyAsync("USAGE", 100, new List<Guid>(), _admin.Id);

        result.IsValid.Should().BeTrue();

        var coupon = await _sut.GetCouponByIdAsync(
            (await Context.Coupons.FirstAsync(c => c.Code == "USAGE")).Id);
        coupon!.TimesUsed.Should().Be(1);
    }
}
