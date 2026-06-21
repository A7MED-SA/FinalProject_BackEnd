using Athary.Application.DTOs.Commerce;
using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Authentication;
using Athary.Application.Interfaces.Commerce;
using Athary.Application.Interfaces.Courses;
using Athary.Application.Interfaces.Notification;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Services.Commerce;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Athary.Infrastructure.Tests.Commerce;

public sealed class PaymentServiceTests : SqliteTestBase
{
    private readonly PaymentService _sut;
    private readonly User _user;
    private readonly Order _order;
    private readonly PaymentMethod _paymentMethod;
    private readonly Mock<IPaymentGateway> _gatewayMock;
    private readonly Mock<IEnrollmentService> _enrollmentMock;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly Mock<IActivityLogService> _activityLogMock;

    public PaymentServiceTests()
    {
        _user = new User { FirstName = "U", LastName = "T", Email = "u@t.com", UserName = "ut" };
        Context.Users.Add(_user);

        var cat = new Category { Name = "Cat", Slug = "cat" };
        Context.Categories.Add(cat);

        var course = new Course
        {
            Title = "C", Slug = "c", Price = 100, CategoryId = cat.Id,
            CreatedBy = _user.Id, Status = CourseStatus.Published, IsPublished = true
        };
        Context.Courses.Add(course);

        _paymentMethod = new PaymentMethod
        {
            Name = "Visa", Provider = "Mock", Type = PaymentMethodType.CreditCard, IsActive = true
        };
        Context.PaymentMethods.Add(_paymentMethod);
        Context.SaveChanges();

        _order = new Order
        {
            UserId = _user.Id,
            OrderNumber = "ORD-TEST-000001",
            SubtotalAmount = 100,
            FinalAmount = 100,
            Status = OrderStatus.Pending
        };
        _order.OrderItems.Add(new OrderItem { CourseId = course.Id, PriceAtPurchase = 100 });
        Context.Orders.Add(_order);
        Context.SaveChanges();

        _gatewayMock = new Mock<IPaymentGateway>();
        _enrollmentMock = new Mock<IEnrollmentService>();
        _notificationMock = new Mock<INotificationService>();
        _activityLogMock = new Mock<IActivityLogService>();

        _sut = new PaymentService(
            Context, _gatewayMock.Object, _enrollmentMock.Object,
            _notificationMock.Object, _activityLogMock.Object);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldSucceed_WhenGatewayReturnsSuccess()
    {
        _gatewayMock
            .Setup(g => g.ProcessPaymentAsync(100, "EGP", _paymentMethod.Id, It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PaymentResult.Succeeded("TXN-001"));

        _enrollmentMock
            .Setup(e => e.EnrollUserAsync(It.IsAny<CreateEnrollmentDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnrollmentResponseDto { Id = Guid.NewGuid() });

        _notificationMock
            .Setup(n => n.CreateAndSendNotificationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Athary.Application.DTOs.Notification.NotificationDto());

        var result = await _sut.ProcessPaymentAsync(_user.Id, _order.Id, _paymentMethod.Id);

        result.Status.Should().Be("Succeeded");
        result.Amount.Should().Be(100);
        result.GatewayTransactionId.Should().Be("TXN-001");
        result.PaymentMethodName.Should().Be("Visa");

        var paidOrder = await Context.Orders.FindAsync(_order.Id);
        paidOrder!.Status.Should().Be(OrderStatus.Completed);

        var payment = await Context.Payments.FirstAsync(p => p.OrderId == _order.Id);
        payment.TransactionRef.Should().Be("TXN-001");
        payment.PaidAt.Should().NotBeNull();

        _enrollmentMock.Verify(e => e.EnrollUserAsync(It.IsAny<CreateEnrollmentDto>(), It.IsAny<CancellationToken>()), Times.Once);
        _notificationMock.Verify(n => n.CreateAndSendNotificationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>(), null, null, It.IsAny<CancellationToken>()), Times.Once);
        _activityLogMock.Verify(a => a.LogActivityAsync(_user.Id, "ProcessPayment", It.IsAny<string>(), "system", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldRecordCouponUsage_WhenOrderHasCoupon()
    {
        var coupon = new Coupon { Code = "TEST", Type = CouponType.Percentage, Value = 10, CreatedBy = _user.Id };
        Context.Coupons.Add(coupon);
        await Context.SaveChangesAsync();

        _order.CouponId = coupon.Id;
        await Context.SaveChangesAsync();

        _gatewayMock
            .Setup(g => g.ProcessPaymentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PaymentResult.Succeeded("TXN-002"));

        _enrollmentMock
            .Setup(e => e.EnrollUserAsync(It.IsAny<CreateEnrollmentDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnrollmentResponseDto { Id = Guid.NewGuid() });

        _notificationMock
            .Setup(n => n.CreateAndSendNotificationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Athary.Application.DTOs.Notification.NotificationDto());

        await _sut.ProcessPaymentAsync(_user.Id, _order.Id, _paymentMethod.Id);

        var usage = await Context.CouponUsages
            .FirstOrDefaultAsync(cu => cu.CouponId == coupon.Id && cu.UserId == _user.Id);
        usage.Should().NotBeNull();
        usage!.OrderId.Should().Be(_order.Id);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldFail_WhenGatewayReturnsFailure()
    {
        _gatewayMock
            .Setup(g => g.ProcessPaymentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PaymentResult.Failed("Insufficient funds"));

        var result = await _sut.ProcessPaymentAsync(_user.Id, _order.Id, _paymentMethod.Id);

        result.Status.Should().Be("Failed");

        var paidOrder = await Context.Orders.FindAsync(_order.Id);
        paidOrder!.Status.Should().Be(OrderStatus.Failed);

        _enrollmentMock.Verify(e => e.EnrollUserAsync(It.IsAny<CreateEnrollmentDto>(), It.IsAny<CancellationToken>()), Times.Never);
        _activityLogMock.Verify(a => a.LogActivityAsync(_user.Id, "ProcessPayment", It.Is<string>(s => s.Contains("failed")), "system", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrow_WhenOrderNotFound()
    {
        await FluentActions.Invoking(() => _sut.ProcessPaymentAsync(_user.Id, Guid.NewGuid(), _paymentMethod.Id))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrow_WhenOrderAlreadyCompleted()
    {
        _order.Status = OrderStatus.Completed;
        await Context.SaveChangesAsync();

        await FluentActions.Invoking(() => _sut.ProcessPaymentAsync(_user.Id, _order.Id, _paymentMethod.Id))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrow_WhenPaymentMethodInactive()
    {
        _paymentMethod.IsActive = false;
        await Context.SaveChangesAsync();

        await FluentActions.Invoking(() => _sut.ProcessPaymentAsync(_user.Id, _order.Id, _paymentMethod.Id))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldNotThrowOnPartialEnrollmentFailure()
    {
        _gatewayMock
            .Setup(g => g.ProcessPaymentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PaymentResult.Succeeded("TXN-003"));

        _enrollmentMock
            .Setup(e => e.EnrollUserAsync(It.IsAny<CreateEnrollmentDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Enrollment failed"));

        _notificationMock
            .Setup(n => n.CreateAndSendNotificationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Athary.Application.DTOs.Notification.NotificationDto());

        var act = async () => await _sut.ProcessPaymentAsync(_user.Id, _order.Id, _paymentMethod.Id);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetActivePaymentMethodsAsync_ShouldReturnOnlyActive()
    {
        var inactive = new PaymentMethod
        {
            Name = "Inactive", Provider = "Mock", Type = PaymentMethodType.BankTransfer, IsActive = false
        };
        Context.PaymentMethods.Add(inactive);
        await Context.SaveChangesAsync();

        var methods = await _sut.GetActivePaymentMethodsAsync();

        methods.Should().HaveCount(1);
        methods.All(m => m.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task GetPaymentHistoryAsync_ShouldReturnOrderPayments()
    {
        _gatewayMock
            .Setup(g => g.ProcessPaymentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PaymentResult.Succeeded("TXN-HIST"));
        _enrollmentMock
            .Setup(e => e.EnrollUserAsync(It.IsAny<CreateEnrollmentDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnrollmentResponseDto { Id = Guid.NewGuid() });
        _notificationMock
            .Setup(n => n.CreateAndSendNotificationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Athary.Application.DTOs.Notification.NotificationDto());

        await _sut.ProcessPaymentAsync(_user.Id, _order.Id, _paymentMethod.Id);

        var history = await _sut.GetPaymentHistoryAsync(_order.Id);

        history.Should().HaveCount(1);
        history.First().GatewayTransactionId.Should().Be("TXN-HIST");
    }
}
