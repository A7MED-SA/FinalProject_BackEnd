using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using backend_project.DTOs.Order;
using backend_project.DTOs.Enrollment;
using backend_project.Models;
using backend_project.Services.Implementations;
using backend_project.Services.Interfaces;
using backend_project.Services.Notifications;
using backend_project.Data;
using CommerceTests.Helpers;

namespace CommerceTests;

public class CommerceFlowIntegrationTests
{
    [Fact]
    public async Task FullFlow_CartToOrderToPayment_CreatesOrderPaymentAndEnrollment()
    {
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var creator = TestDbContextFactory.CreateUser();
        var course = TestDbContextFactory.CreateCourse(creatorId: creator.Id, price: 150);
        context.Users.AddRange(creator, TestDbContextFactory.CreateUser(userId));
        context.Courses.Add(course);

        var cart = TestDbContextFactory.CreateCart(userId);
        context.Carts.Add(cart);
        context.CartItems.Add(TestDbContextFactory.CreateCartItem(cart.Id, course.Id, 150));
        await context.SaveChangesAsync();

        var paymentMethod = TestDbContextFactory.CreatePaymentMethod();
        context.PaymentMethods.Add(paymentMethod);
        await context.SaveChangesAsync();

        var mockPaymentGateway = new Mock<IPaymentGateway>();
        mockPaymentGateway.Setup(g => g.ProcessPaymentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>?>()))
            .ReturnsAsync(PaymentResult.Succeeded("TXN-INT-TEST"));

        var mockEnrollment = new Mock<IEnrollmentService>();
        EnrollmentResponseDto? capturedEnrollment = null;
        mockEnrollment.Setup(e => e.EnrollUserAsync(It.IsAny<CreateEnrollmentDto>()))
            .Callback<CreateEnrollmentDto>(dto => capturedEnrollment = new EnrollmentResponseDto
            {
                UserId = dto.UserId,
                CourseId = dto.CourseId,
                Status = EnrollmentStatus.InProgress
            })
            .ReturnsAsync(() => capturedEnrollment!);

        var mockNotif = new Mock<INotificationService>();
        var mockActivityLog = new Mock<IActivityLogService>();
        mockActivityLog.Setup(a => a.LogActivityAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var mockCoupon = new Mock<ICouponService>();
        var mockPaymentSvc = new Mock<IPaymentService>();

        var orderService = new OrderService(context, mockPaymentSvc.Object, mockCoupon.Object, mockActivityLog.Object);
        var order = await orderService.CreateOrderAsync(userId, new CreateOrderRequest());

        var paymentService = new PaymentService(context, mockPaymentGateway.Object, mockEnrollment.Object, mockNotif.Object, mockActivityLog.Object);
        var paymentResult = await paymentService.ProcessPaymentAsync(userId, order.Id, paymentMethod.Id);

        order.Status.Should().Be("Pending");
        paymentResult.Status.Should().Be("Succeeded");
        order.FinalAmount.Should().Be(150);

        var updatedOrder = await context.Orders.FindAsync(order.Id);
        updatedOrder!.Status.Should().Be(OrderStatus.Completed);

        mockEnrollment.Verify(e => e.EnrollUserAsync(It.Is<CreateEnrollmentDto>(
            d => d.UserId == userId && d.CourseId == course.Id)), Times.Once);

        var cartItems = await context.CartItems.Where(ci => ci.CartId == cart.Id).ToListAsync();
        cartItems.Should().BeEmpty();

        var payment = await context.Payments.FirstOrDefaultAsync(p => p.OrderId == order.Id);
        payment.Should().NotBeNull();
        payment!.Status.Should().Be(PaymentStatus.Succeeded);
    }
}
