using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using backend_project.DTOs.Refund;
using backend_project.Models;
using backend_project.Services.Implementations;
using backend_project.Services.Interfaces;
using backend_project.Services.Notifications;
using backend_project.Data;
using CommerceTests.Helpers;

namespace CommerceTests;

public class RefundServiceTests
{
    private static async Task<(ApplicationDbContext ctx, Guid userId, Guid paymentId, Guid orderId)> SetupRefundablePayment()
    {
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var creator = TestDbContextFactory.CreateUser();
        var course = TestDbContextFactory.CreateCourse(creatorId: creator.Id);
        context.Users.AddRange(creator, TestDbContextFactory.CreateUser(userId));
        context.Courses.Add(course);

        var order = new Order
        {
            OrderNumber = "ORD-REF-TEST",
            UserId = userId,
            SubtotalAmount = 100,
            FinalAmount = 100,
            Status = OrderStatus.Completed,
            Currency = "EGP"
        };
        context.Orders.Add(order);

        var paymentMethod = TestDbContextFactory.CreatePaymentMethod();
        context.PaymentMethods.Add(paymentMethod);

        var payment = new Payment
        {
            UserId = userId,
            OrderId = order.Id,
            PaymentMethodId = paymentMethod.Id,
            Amount = 100,
            Status = PaymentStatus.Succeeded,
            TransactionRef = "TXN-REF-TEST",
            Currency = PaymentCurrency.EGP
        };
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        return (context, userId, payment.Id, order.Id);
    }

    [Fact]
    public async Task RequestRefund_ValidPayment_CreatesRequest()
    {
        var (context, userId, paymentId, _) = await SetupRefundablePayment();

        var mockNotif = new Mock<INotificationService>();
        var mockEnrollment = new Mock<IEnrollmentService>();
        var mockActivityLog = new Mock<IActivityLogService>();
        var service = new RefundService(context, mockEnrollment.Object, mockNotif.Object, mockActivityLog.Object);

        var result = await service.RequestRefundAsync(userId, new RequestRefundRequest
        {
            PaymentId = paymentId,
            Reason = "Course not as expected"
        });

        result.Status.Should().Be("Requested");
        result.Reason.Should().Be("Course not as expected");
    }

    [Fact]
    public async Task RequestRefund_FreeCourse_NotApplicable()
    {
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        context.Users.Add(TestDbContextFactory.CreateUser(userId));

        var creator = TestDbContextFactory.CreateUser();
        var course = TestDbContextFactory.CreateCourse(creatorId: creator.Id);
        context.Users.Add(creator);
        context.Courses.Add(course);

        var order = new Order
        {
            OrderNumber = "ORD-FREE",
            UserId = userId,
            SubtotalAmount = 0,
            FinalAmount = 0,
            Status = OrderStatus.Completed,
            Currency = "EGP"
        };
        context.Orders.Add(order);

        var paymentMethod = TestDbContextFactory.CreatePaymentMethod();
        context.PaymentMethods.Add(paymentMethod);

        var payment = new Payment
        {
            UserId = userId,
            OrderId = order.Id,
            PaymentMethodId = paymentMethod.Id,
            Amount = 0,
            Status = PaymentStatus.Succeeded,
            TransactionRef = "TXN-FREE",
            Currency = PaymentCurrency.EGP
        };
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var mockNotif = new Mock<INotificationService>();
        var mockEnrollment = new Mock<IEnrollmentService>();
        var mockActivityLog = new Mock<IActivityLogService>();
        var service = new RefundService(context, mockEnrollment.Object, mockNotif.Object, mockActivityLog.Object);

        var result = await service.RequestRefundAsync(userId, new RequestRefundRequest
        {
            PaymentId = payment.Id,
            Reason = "Changed mind"
        });

        result.Status.Should().Be("Requested");
    }

    [Fact]
    public async Task ApproveRefund_UpdatesOrderToRefunded()
    {
        var (context, userId, paymentId, orderId) = await SetupRefundablePayment();
        var adminId = Guid.NewGuid();

        var refund = new Refund
        {
            PaymentId = paymentId,
            Amount = 100,
            Status = RefundStatus.Requested,
            Reason = "Not satisfied",
            RequestedAt = DateTime.UtcNow
        };
        context.Refunds.Add(refund);
        await context.SaveChangesAsync();

        var mockNotif = new Mock<INotificationService>();
        var mockEnrollment = new Mock<IEnrollmentService>();
        var mockActivityLog = new Mock<IActivityLogService>();
        var service = new RefundService(context, mockEnrollment.Object, mockNotif.Object, mockActivityLog.Object);

        var result = await service.ApproveRefundAsync(adminId, new ProcessRefundRequest { RefundId = refund.Id });
        result.Status.Should().Be("Approved");

        var updatedOrder = await context.Orders.FindAsync(orderId);
        updatedOrder!.Status.Should().Be(OrderStatus.Refunded);
    }

    [Fact]
    public async Task ApproveRefund_UpdatesEnrollmentToRefunded()
    {
        var (context, userId, paymentId, orderId) = await SetupRefundablePayment();
        var adminId = Guid.NewGuid();

        var course = await context.Courses.FirstAsync();
        context.Enrollments.Add(new Enrollment
        {
            UserId = userId,
            CourseId = course.Id,
            Status = EnrollmentStatus.InProgress,
            Source = EnrollmentSource.Purchase
        });

        var refund = new Refund
        {
            PaymentId = paymentId,
            Amount = 100,
            Status = RefundStatus.Requested,
            RequestedAt = DateTime.UtcNow
        };
        context.Refunds.Add(refund);
        await context.SaveChangesAsync();

        var mockNotif = new Mock<INotificationService>();
        var mockEnrollment = new Mock<IEnrollmentService>();
        var mockActivityLog = new Mock<IActivityLogService>();
        var service = new RefundService(context, mockEnrollment.Object, mockNotif.Object, mockActivityLog.Object);

        await service.ApproveRefundAsync(adminId, new ProcessRefundRequest { RefundId = refund.Id });

        var order = await context.Orders.FindAsync(orderId);
        order!.Status.Should().Be(OrderStatus.Refunded);
        order.Status.Should().Be(OrderStatus.Refunded);
    }

    [Fact]
    public async Task RejectRefund_KeepsOrderCompleted()
    {
        var (context, userId, paymentId, orderId) = await SetupRefundablePayment();
        var adminId = Guid.NewGuid();

        var refund = new Refund
        {
            PaymentId = paymentId,
            Amount = 100,
            Status = RefundStatus.Requested,
            RequestedAt = DateTime.UtcNow
        };
        context.Refunds.Add(refund);
        await context.SaveChangesAsync();

        var mockNotif = new Mock<INotificationService>();
        var mockEnrollment = new Mock<IEnrollmentService>();
        var mockActivityLog = new Mock<IActivityLogService>();
        var service = new RefundService(context, mockEnrollment.Object, mockNotif.Object, mockActivityLog.Object);

        var result = await service.RejectRefundAsync(adminId, new ProcessRefundRequest { RefundId = refund.Id });
        result.Status.Should().Be("Rejected");

        var updatedOrder = await context.Orders.FindAsync(orderId);
        updatedOrder!.Status.Should().Be(OrderStatus.Completed);
    }
}
