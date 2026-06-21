using Athary.Application.DTOs.Commerce;
using Athary.Application.Interfaces.Commerce;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Services.Commerce;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Athary.Infrastructure.Tests.Commerce;

public sealed class OrderServiceTests : SqliteTestBase
{
    private readonly OrderService _sut;
    private readonly User _user;
    private readonly Course _course1;
    private readonly Course _course2;
    private readonly Mock<ICouponService> _couponMock;

    public OrderServiceTests()
    {
        _user = new User { FirstName = "U", LastName = "T", Email = "u@t.com", UserName = "ut" };
        Context.Users.Add(_user);

        var cat = new Category { Name = "Cat", Slug = "cat" };
        Context.Categories.Add(cat);
        Context.SaveChanges();

        _course1 = new Course
        {
            Title = "C1", Slug = "c1", Price = 100, CategoryId = cat.Id,
            CreatedBy = _user.Id, Status = CourseStatus.Published, IsPublished = true
        };
        _course2 = new Course
        {
            Title = "C2", Slug = "c2", Price = 50, CategoryId = cat.Id,
            CreatedBy = _user.Id, Status = CourseStatus.Published, IsPublished = true
        };
        Context.Courses.AddRange(_course1, _course2);
        Context.SaveChanges();

        _couponMock = new Mock<ICouponService>();
        _sut = new OrderService(Context, _couponMock.Object);
    }

    private async Task SeedCartAsync()
    {
        var cart = new Cart { UserId = _user.Id };
        cart.CartItems.Add(new CartItem { CourseId = _course1.Id, PriceSnapshot = _course1.Price });
        cart.CartItems.Add(new CartItem { CourseId = _course2.Id, PriceSnapshot = _course2.Price });
        Context.Carts.Add(cart);
        await Context.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldCreateOrder_AndClearCart()
    {
        await SeedCartAsync();

        var result = await _sut.CreateOrderAsync(_user.Id, new CreateOrderRequest());

        result.OrderNumber.Should().StartWith("ORD-");
        result.Subtotal.Should().Be(150);
        result.DiscountAmount.Should().Be(0);
        result.FinalAmount.Should().Be(150);
        result.ItemCount.Should().Be(2);
        result.Status.Should().Be("Pending");

        var saved = await Context.Orders
            .Include(o => o.OrderItems)
            .FirstAsync(o => o.Id == result.Id);
        saved.OrderItems.Should().HaveCount(2);
        saved.UserId.Should().Be(_user.Id);

        var cartExists = await Context.Carts.AnyAsync(c => c.UserId == _user.Id);
        cartExists.Should().BeFalse();
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldApplyCouponDiscount()
    {
        await SeedCartAsync();

        _couponMock
            .Setup(c => c.ValidateAndApplyAsync("SAVE10", 150, It.Is<List<Guid>>(l => l.Count == 2), _user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CouponValidationResult { IsValid = true, DiscountAmount = 15, FinalAmount = 135 });

        var request = new CreateOrderRequest { CouponCode = "SAVE10" };
        var result = await _sut.CreateOrderAsync(_user.Id, request);

        result.DiscountAmount.Should().Be(15);
        result.FinalAmount.Should().Be(135);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldThrow_WhenCartEmpty()
    {
        await FluentActions.Invoking(() => _sut.CreateOrderAsync(_user.Id, new CreateOrderRequest()))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldThrow_WhenCouponInvalid()
    {
        await SeedCartAsync();

        _couponMock
            .Setup(c => c.ValidateAndApplyAsync("BAD", 150, It.IsAny<List<Guid>>(), _user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CouponValidationResult { IsValid = false, Message = "الكوبون غير صالح." });

        var request = new CreateOrderRequest { CouponCode = "BAD" };
        await FluentActions.Invoking(() => _sut.CreateOrderAsync(_user.Id, request))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetOrdersAsync_ShouldReturnUserOrders_OrderedByDate()
    {
        await SeedCartAsync();
        var order = await _sut.CreateOrderAsync(_user.Id, new CreateOrderRequest());

        var orders = await _sut.GetOrdersAsync(_user.Id);

        orders.Should().HaveCount(1);
        orders.First().Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task GetOrderDetailsAsync_ShouldReturnFullDetails()
    {
        await SeedCartAsync();
        var created = await _sut.CreateOrderAsync(_user.Id, new CreateOrderRequest());

        var detail = await _sut.GetOrderDetailsAsync(_user.Id, created.Id);

        detail.Items.Should().HaveCount(2);
        detail.Items.Any(i => i.CourseId == _course1.Id).Should().BeTrue();
        detail.Items.Any(i => i.CourseId == _course2.Id).Should().BeTrue();
        detail.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task GetOrderDetailsAsync_ShouldThrow_WhenNotOwnedByUser()
    {
        await SeedCartAsync();
        var created = await _sut.CreateOrderAsync(_user.Id, new CreateOrderRequest());

        var otherUserId = Guid.NewGuid();
        await FluentActions.Invoking(() => _sut.GetOrderDetailsAsync(otherUserId, created.Id))
            .Should().ThrowAsync<KeyNotFoundException>();
    }
}
