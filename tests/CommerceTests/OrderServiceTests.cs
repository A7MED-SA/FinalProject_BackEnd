using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using backend_project.DTOs.Order;
using backend_project.Models;
using backend_project.Services.Implementations;
using backend_project.Services.Interfaces;
using Moq;
using backend_project.Data;
using CommerceTests.Helpers;

namespace CommerceTests;

public class OrderServiceTests
{
    private static async Task<(ApplicationDbContext ctx, Guid userId, Guid courseId)> SetupCartWithItems()
    {
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var creator = TestDbContextFactory.CreateUser();
        var course = TestDbContextFactory.CreateCourse(creatorId: creator.Id);
        context.Users.AddRange(creator, TestDbContextFactory.CreateUser(userId));
        context.Courses.Add(course);
        var cart = TestDbContextFactory.CreateCart(userId);
        context.Carts.Add(cart);
        context.CartItems.Add(TestDbContextFactory.CreateCartItem(cart.Id, course.Id, 150));
        await context.SaveChangesAsync();
        return (context, userId, course.Id);
    }

    [Fact]
    public async Task CreateOrder_ValidCart_CreatesPendingOrder()
    {
        var (context, userId, _) = await SetupCartWithItems();

        var mockPayment = new Mock<IPaymentService>();
        var mockCoupon = new Mock<ICouponService>();
        var mockActivityLog = new Mock<IActivityLogService>();
        var service = new OrderService(context, mockPayment.Object, mockCoupon.Object, mockActivityLog.Object);

        var result = await service.CreateOrderAsync(userId, new CreateOrderRequest());

        result.Status.Should().Be("Pending");
        result.Subtotal.Should().Be(150);
        result.FinalAmount.Should().Be(150);
        result.ItemCount.Should().Be(1);
        result.OrderNumber.Should().StartWith("ORD-");
    }

    [Fact]
    public async Task CreateOrder_EmptyCart_ThrowsException()
    {
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        context.Users.Add(TestDbContextFactory.CreateUser(userId));
        await context.SaveChangesAsync();

        var mockPayment = new Mock<IPaymentService>();
        var mockCoupon = new Mock<ICouponService>();
        var mockActivityLog = new Mock<IActivityLogService>();
        var service = new OrderService(context, mockPayment.Object, mockCoupon.Object, mockActivityLog.Object);

        await FluentActions.Awaiting(() => service.CreateOrderAsync(userId, new CreateOrderRequest()))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cart is empty.");
    }
}
