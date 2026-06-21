using Athary.Application.DTOs.Commerce;
using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Authentication;
using Athary.Application.Interfaces.Commerce;
using Athary.Application.Interfaces.Courses;
using Athary.Application.Interfaces.Notification;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.Commerce;

public sealed class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IEnrollmentService _enrollmentService;
    private readonly INotificationService _notificationService;
    private readonly IActivityLogService _activityLogService;

    public PaymentService(
        ApplicationDbContext context,
        IPaymentGateway paymentGateway,
        IEnrollmentService enrollmentService,
        INotificationService notificationService,
        IActivityLogService activityLogService)
    {
        _context = context;
        _paymentGateway = paymentGateway;
        _enrollmentService = enrollmentService;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
    }

    public async Task<PaymentResponseDto> ProcessPaymentAsync(Guid userId, Guid orderId, Guid paymentMethodId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Coupon)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("الطلب غير موجود.");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("لا يمكن معالجة الدفع لطلب تمت معالجته بالفعل.");

        var paymentMethod = await _context.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.Id == paymentMethodId && pm.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("طريقة الدفع غير متاحة.");

        var currency = "EGP";

        var metadata = new Dictionary<string, string>
        {
            ["OrderId"] = order.Id.ToString(),
            ["UserId"] = userId.ToString(),
            ["OrderNumber"] = order.OrderNumber
        };

        var payment = new Payment
        {
            UserId = userId,
            OrderId = orderId,
            PaymentMethodId = paymentMethodId,
            Amount = order.FinalAmount,
            Currency = PaymentCurrency.EGP,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        var result = await _paymentGateway.ProcessPaymentAsync(
            order.FinalAmount, currency, paymentMethodId, metadata, cancellationToken);

                payment.GatewayResponse = result.GatewayMessage;

            if (result.Status == PaymentResultStatus.Succeeded)
            {
                payment.Status = PaymentStatus.Succeeded;
                payment.TransactionRef = result.TransactionRef;
                payment.PaidAt = result.ProcessedAt;
                order.Status = OrderStatus.Completed;
                order.UpdatedAt = DateTime.UtcNow;

                if (order.CouponId.HasValue)
                {
                    _context.CouponUsages.Add(new CouponUsage
                    {
                        CouponId = order.CouponId.Value,
                        UserId = userId,
                        OrderId = order.Id
                    });
                }

                foreach (var item in order.OrderItems)
            {
                try
                {
                    var enrollmentDto = new CreateEnrollmentDto
                    {
                        CourseId = item.CourseId,
                        UserId = userId,
                        Source = EnrollmentSource.Purchase
                    };

                    await _enrollmentService.EnrollUserAsync(enrollmentDto, cancellationToken);
                }
                catch
                {
                    // Log and continue — partial enrollment is acceptable
                }
            }

            await _notificationService.CreateAndSendNotificationAsync(
                userId,
                "تم الدفع بنجاح",
                $"تم تأكيد دفع طلب #{order.OrderNumber} بنجاح. يمكنك الآن الوصول إلى الدورات المشتراة.",
                NotificationType.Payment,
                cancellationToken: cancellationToken);

            await _activityLogService.LogActivityAsync(
                userId, "ProcessPayment", $"Payment {result.TransactionRef} for order {order.OrderNumber} succeeded.",
                "system", cancellationToken: cancellationToken);
        }
        else
        {
            payment.Status = PaymentStatus.Failed;
            order.Status = OrderStatus.Failed;
            order.UpdatedAt = DateTime.UtcNow;

            await _activityLogService.LogActivityAsync(
                userId, "ProcessPayment", $"Payment for order {order.OrderNumber} failed: {result.GatewayMessage}",
                "system", cancellationToken: cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapPaymentToDto(payment, paymentMethod.Name);
    }

    public async Task<IEnumerable<PaymentResponseDto>> GetPaymentHistoryAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .AsNoTracking()
            .Include(p => p.PaymentMethod)
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentResponseDto
            {
                Id = p.Id,
                OrderId = p.OrderId,
                Amount = p.Amount,
                Status = p.Status.ToString(),
                GatewayTransactionId = p.TransactionRef,
                GatewayResponse = p.GatewayResponse,
                PaymentMethodName = p.PaymentMethod.Name,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentMethodResponse> CreatePaymentMethodAsync(Guid adminUserId, CreatePaymentMethodRequest request, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<PaymentMethodType>(request.Type, true, out var type))
            throw new InvalidOperationException("نوع طريقة الدفع غير صالح.");

        var method = new PaymentMethod
        {
            Name = request.Name,
            Provider = request.Provider,
            Type = type,
            IsActive = true,
            Configuration = request.Configuration
        };

        _context.PaymentMethods.Add(method);
        await _context.SaveChangesAsync(cancellationToken);

        return MapPaymentMethod(method);
    }

    public async Task<bool> TogglePaymentMethodAsync(Guid paymentMethodId, CancellationToken cancellationToken = default)
    {
        var method = await _context.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.Id == paymentMethodId, cancellationToken);

        if (method is null)
            return false;

        method.IsActive = !method.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<PaymentMethodResponse>> GetActivePaymentMethodsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PaymentMethods
            .AsNoTracking()
            .Where(pm => pm.IsActive)
            .OrderBy(pm => pm.Name)
            .Select(pm => new PaymentMethodResponse
            {
                Id = pm.Id,
                Name = pm.Name,
                Provider = pm.Provider,
                Type = pm.Type.ToString(),
                IsActive = pm.IsActive,
                Configuration = pm.Configuration
            })
            .ToListAsync(cancellationToken);
    }

    private static PaymentResponseDto MapPaymentToDto(Payment payment, string? methodName) => new()
    {
        Id = payment.Id,
        OrderId = payment.OrderId,
        Amount = payment.Amount,
        Status = payment.Status.ToString(),
        GatewayTransactionId = payment.TransactionRef,
        GatewayResponse = payment.GatewayResponse,
        PaymentMethodName = methodName,
        CreatedAt = payment.CreatedAt
    };

    private static PaymentMethodResponse MapPaymentMethod(PaymentMethod pm) => new()
    {
        Id = pm.Id,
        Name = pm.Name,
        Provider = pm.Provider,
        Type = pm.Type.ToString(),
        IsActive = pm.IsActive,
        Configuration = pm.Configuration
    };
}
