using Athary.Application.DTOs.Commerce;
using Athary.Application.Interfaces.Authentication;
using Athary.Application.Interfaces.Notification;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Services.Commerce;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Athary.Infrastructure.Tests.Commerce;

public sealed class RefundServiceTests : SqliteTestBase
{
    private readonly RefundService _sut;
    private readonly User _user;
    private readonly User _admin;
    private readonly Payment _payment;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly Mock<IActivityLogService> _activityLogMock;

    public RefundServiceTests()
    {
        _user = new User { FirstName = "U", LastName = "T", Email = "u@t.com", UserName = "ut" };
        _admin = new User { FirstName = "Admin", LastName = "X", Email = "ax@t.com", UserName = "ax" };
        Context.Users.AddRange(_user, _admin);

        var cat = new Category { Name = "Cat", Slug = "cat" };
        Context.Categories.Add(cat);

        var course = new Course
        {
            Title = "C", Slug = "c", Price = 100, CategoryId = cat.Id,
            CreatedBy = _user.Id, Status = CourseStatus.Published, IsPublished = true
        };
        Context.Courses.Add(course);

        var order = new Order
        {
            UserId = _user.Id, OrderNumber = "ORD-REFUND", SubtotalAmount = 100,
            FinalAmount = 100, Status = OrderStatus.Completed
        };
        order.OrderItems.Add(new OrderItem { CourseId = course.Id, PriceAtPurchase = 100 });
        Context.Orders.Add(order);

        var paymentMethod = new PaymentMethod
        {
            Name = "Visa", Provider = "Mock", Type = PaymentMethodType.CreditCard, IsActive = true
        };
        Context.PaymentMethods.Add(paymentMethod);

        _payment = new Payment
        {
            UserId = _user.Id, OrderId = order.Id, PaymentMethodId = paymentMethod.Id,
            Amount = 100, Status = PaymentStatus.Succeeded,
            TransactionRef = "TXN-REFUND"
        };
        Context.Payments.Add(_payment);
        Context.SaveChanges();

        _notificationMock = new Mock<INotificationService>();
        _activityLogMock = new Mock<IActivityLogService>();

        _sut = new RefundService(Context, _notificationMock.Object, _activityLogMock.Object);
    }

    [Fact]
    public async Task RequestRefundAsync_ShouldCreateRefundRequest()
    {
        var result = await _sut.RequestRefundAsync(_user.Id, new RequestRefundRequest
        {
            PaymentId = _payment.Id,
            Reason = "Not satisfied"
        });

        result.Status.Should().Be("Requested");
        result.Amount.Should().Be(100);
        result.Reason.Should().Be("Not satisfied");
        result.OrderNumber.Should().Be("ORD-REFUND");
    }

    [Fact]
    public async Task RequestRefundAsync_ShouldThrow_WhenPaymentNotFound()
    {
        await FluentActions.Invoking(() => _sut.RequestRefundAsync(_user.Id, new RequestRefundRequest { PaymentId = Guid.NewGuid() }))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task RequestRefundAsync_ShouldThrow_WhenPaymentNotSucceeded()
    {
        _payment.Status = PaymentStatus.Failed;
        await Context.SaveChangesAsync();

        await FluentActions.Invoking(() => _sut.RequestRefundAsync(_user.Id, new RequestRefundRequest { PaymentId = _payment.Id }))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RequestRefundAsync_ShouldThrow_WhenDuplicateRequest()
    {
        await _sut.RequestRefundAsync(_user.Id, new RequestRefundRequest { PaymentId = _payment.Id });

        await FluentActions.Invoking(() => _sut.RequestRefundAsync(_user.Id, new RequestRefundRequest { PaymentId = _payment.Id }))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ApproveRefundAsync_ShouldUpdateStatus_AndRevokeEnrollment()
    {
        var requested = await _sut.RequestRefundAsync(_user.Id, new RequestRefundRequest
        {
            PaymentId = _payment.Id, Reason = "Test"
        });

        var enrollment = new Enrollment
        {
            UserId = _user.Id,
            CourseId = Context.Courses.First().Id,
            Status = EnrollmentStatus.InProgress
        };
        Context.Enrollments.Add(enrollment);
        await Context.SaveChangesAsync();

        _notificationMock
            .Setup(n => n.CreateAndSendNotificationAsync(_user.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Athary.Application.DTOs.Notification.NotificationDto());

        var result = await _sut.ApproveRefundAsync(_admin.Id, new ProcessRefundRequest
        {
            RefundId = requested.Id, AdminNotes = "Approved"
        });

        result.Status.Should().Be("Processed");

        var refund = await Context.Refunds.FindAsync(requested.Id);
        refund!.ProcessedBy.Should().Be(_admin.Id);

        var updatedPayment = await Context.Payments.FindAsync(_payment.Id);
        updatedPayment!.Status.Should().Be(PaymentStatus.Pending);

        var updatedEnrollment = await Context.Enrollments.FindAsync(enrollment.Id);
        updatedEnrollment!.Status.Should().Be(EnrollmentStatus.Refunded);
    }

    [Fact]
    public async Task ApproveRefundAsync_ShouldThrow_WhenRefundNotFound()
    {
        await FluentActions.Invoking(() => _sut.ApproveRefundAsync(_admin.Id, new ProcessRefundRequest { RefundId = Guid.NewGuid() }))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task RejectRefundAsync_ShouldUpdateStatus_AndNotifyUser()
    {
        var requested = await _sut.RequestRefundAsync(_user.Id, new RequestRefundRequest
        {
            PaymentId = _payment.Id, Reason = "Test"
        });

        _notificationMock
            .Setup(n => n.CreateAndSendNotificationAsync(_user.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Athary.Application.DTOs.Notification.NotificationDto());

        var result = await _sut.RejectRefundAsync(_admin.Id, new ProcessRefundRequest
        {
            RefundId = requested.Id, AdminNotes = "Not eligible"
        });

        result.Status.Should().Be("Rejected");

        var refund = await Context.Refunds.FindAsync(requested.Id);
        refund!.ProcessedBy.Should().Be(_admin.Id);
        refund.ProcessedAt.Should().NotBeNull();

        _notificationMock.Verify(n => n.CreateAndSendNotificationAsync(
            _user.Id, It.IsAny<string>(), It.Is<string>(s => s.Contains("رفض")),
            NotificationType.Payment, null, null, It.IsAny<CancellationToken>()), Times.Once);

        var payment = await Context.Payments.FindAsync(_payment.Id);
        payment!.Status.Should().Be(PaymentStatus.Succeeded);
    }

    [Fact]
    public async Task GetUserRefundsAsync_ShouldReturnUserRefunds()
    {
        await _sut.RequestRefundAsync(_user.Id, new RequestRefundRequest
        {
            PaymentId = _payment.Id, Reason = "Reason"
        });

        var refunds = await _sut.GetUserRefundsAsync(_user.Id);

        refunds.Should().HaveCount(1);
        refunds.First().Reason.Should().Be("Reason");
    }

    [Fact]
    public async Task GetAllRefundsAsync_ShouldReturnAll_WithOptionalStatusFilter()
    {
        await _sut.RequestRefundAsync(_user.Id, new RequestRefundRequest
        {
            PaymentId = _payment.Id, Reason = "R1"
        });

        var all = await _sut.GetAllRefundsAsync(null);
        all.Should().HaveCount(1);

        var requested = await _sut.GetAllRefundsAsync(RefundStatus.Requested);
        requested.Should().HaveCount(1);

        var approved = await _sut.GetAllRefundsAsync(RefundStatus.Approved);
        approved.Should().BeEmpty();
    }
}
