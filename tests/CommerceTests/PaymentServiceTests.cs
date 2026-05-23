using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using backend_project.DTOs.Enrollment;
using backend_project.Models;
using backend_project.Services.Implementations;
using backend_project.Services.Interfaces;
using backend_project.Services.Notifications;
using backend_project.Data;
using CommerceTests.Helpers;

namespace CommerceTests;

public class PaymentServiceTests
{
    private static async Task<(ApplicationDbContext ctx, Guid userId, Guid orderId, Guid paymentMethodId)> SetupPendingOrder()
    {
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var creator = TestDbContextFactory.CreateUser();
        var course = TestDbContextFactory.CreateCourse(creatorId: creator.Id);
        context.Users.AddRange(creator, TestDbContextFactory.CreateUser(userId));

        var paymentMethod = TestDbContextFactory.CreatePaymentMethod();
        context.PaymentMethods.Add(paymentMethod);
        context.Courses.Add(course);

        var order = new Order
        {
            OrderNumber = "ORD-TEST-PMT",
            UserId = userId,
            SubtotalAmount = 100,
            FinalAmount = 100,
            Status = OrderStatus.Pending,
            Currency = "EGP"
        };
        context.Orders.Add(order);
        context.OrderItems.Add(new OrderItem { OrderId = order.Id, CourseId = course.Id, PriceAtPurchase = 100 });
        await context.SaveChangesAsync();

        return (context, userId, order.Id, paymentMethod.Id);
    }

    [Fact]
    public async Task ProcessPayment_Successful_UpdatesOrderToCompleted()
    {
        var (context, userId, orderId, pmId) = await SetupPendingOrder();

        var mockGateway = new Mock<IPaymentGateway>();
        mockGateway.Setup(g => g.ProcessPaymentAsync(100, "EGP", pmId, null))
            .ReturnsAsync(PaymentResult.Succeeded("TXN-001"));

        var mockEnrollment = new Mock<IEnrollmentService>();
        mockEnrollment.Setup(e => e.EnrollUserAsync(It.IsAny<CreateEnrollmentDto>()))
            .ReturnsAsync(new EnrollmentResponseDto());

        var mockNotif = new Mock<INotificationService>();
        var mockActivity = new Mock<IActivityLogService>();

        var service = new PaymentService(context, mockGateway.Object, mockEnrollment.Object, mockNotif.Object, mockActivity.Object);

        var result = await service.ProcessPaymentAsync(userId, orderId, pmId);
        result.Status.Should().Be("Succeeded");

        var updatedOrder = await context.Orders.FindAsync(orderId);
        updatedOrder!.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task ProcessPayment_Successful_CreatesEnrollment()
    {
        var (context, userId, orderId, pmId) = await SetupPendingOrder();

        var mockGateway = new Mock<IPaymentGateway>();
        mockGateway.Setup(g => g.ProcessPaymentAsync(100, "EGP", pmId, null))
            .ReturnsAsync(PaymentResult.Succeeded("TXN-002"));

        var enrollmentCreated = false;
        var mockEnrollment = new Mock<IEnrollmentService>();
        mockEnrollment.Setup(e => e.EnrollUserAsync(It.IsAny<CreateEnrollmentDto>()))
            .Callback<CreateEnrollmentDto>(dto =>
            {
                dto.UserId.Should().Be(userId);
                dto.Source.Should().Be(EnrollmentSource.Purchase);
                enrollmentCreated = true;
            })
            .ReturnsAsync(new EnrollmentResponseDto());

        var mockNotif = new Mock<INotificationService>();
        var mockActivity = new Mock<IActivityLogService>();

        var service = new PaymentService(context, mockGateway.Object, mockEnrollment.Object, mockNotif.Object, mockActivity.Object);
        await service.ProcessPaymentAsync(userId, orderId, pmId);

        enrollmentCreated.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessPayment_Failed_KeepsOrderPending()
    {
        var (context, userId, orderId, pmId) = await SetupPendingOrder();

        var mockGateway = new Mock<IPaymentGateway>();
        mockGateway.Setup(g => g.ProcessPaymentAsync(100, "EGP", pmId, null))
            .ReturnsAsync(PaymentResult.Failed("Insufficient funds"));

        var mockEnrollment = new Mock<IEnrollmentService>();
        var mockNotif = new Mock<INotificationService>();
        var mockActivity = new Mock<IActivityLogService>();

        var service = new PaymentService(context, mockGateway.Object, mockEnrollment.Object, mockNotif.Object, mockActivity.Object);

        var result = await service.ProcessPaymentAsync(userId, orderId, pmId);
        result.Status.Should().Be("Failed");

        var updatedOrder = await context.Orders.FindAsync(orderId);
        updatedOrder!.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public async Task ProcessPayment_ThreeAttempts_CancelsOrder()
    {
        var (context, userId, orderId, pmId) = await SetupPendingOrder();

        for (int i = 0; i < 3; i++)
        {
            context.Payments.Add(new Payment
            {
                UserId = userId,
                OrderId = orderId,
                PaymentMethodId = pmId,
                Amount = 100,
                Status = PaymentStatus.Failed,
                TransactionRef = $"FAIL-{i}",
                Currency = PaymentCurrency.EGP
            });
        }
        await context.SaveChangesAsync();

        var mockGateway = new Mock<IPaymentGateway>();
        var mockEnrollment = new Mock<IEnrollmentService>();
        var mockNotif = new Mock<INotificationService>();
        var mockActivity = new Mock<IActivityLogService>();

        var service = new PaymentService(context, mockGateway.Object, mockEnrollment.Object, mockNotif.Object, mockActivity.Object);

        await FluentActions.Awaiting(() => service.ProcessPaymentAsync(userId, orderId, pmId))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Maximum payment attempts (3) reached. Order has been cancelled.");

        var updatedOrder = await context.Orders.FindAsync(orderId);
        updatedOrder!.Status.Should().Be(OrderStatus.Failed);
    }
}
