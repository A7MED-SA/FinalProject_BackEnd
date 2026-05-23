using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using backend_project.Data;
using backend_project.DTOs.Enrollment;
using backend_project.DTOs.Payment;
using backend_project.Models;
using backend_project.Services.Interfaces;
using backend_project.Services.Notifications;

namespace backend_project.Services.Implementations;

public class PaymentService : IPaymentService
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

    public async Task<PaymentResponseDto> ProcessPaymentAsync(Guid userId, Guid orderId, Guid paymentMethodId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
            throw new KeyNotFoundException("Order not found.");

        if (order.Status == OrderStatus.Completed)
            throw new InvalidOperationException("Order is already completed.");

        if (order.Status == OrderStatus.Refunded)
            throw new InvalidOperationException("Cannot pay for a refunded order.");

        var failedAttempts = order.Payments.Count(p => p.Status == PaymentStatus.Failed);
        if (failedAttempts >= 3)
        {
            order.Status = OrderStatus.Failed;
            await _context.SaveChangesAsync();
            throw new InvalidOperationException("Maximum payment attempts (3) reached. Order has been cancelled.");
        }

        var paymentMethod = await _context.PaymentMethods.FindAsync(paymentMethodId);
        if (paymentMethod == null)
            throw new KeyNotFoundException("Payment method not found.");

        if (!paymentMethod.IsActive)
            throw new InvalidOperationException("Payment method is not active.");

        var attemptNumber = failedAttempts + 1;
        var transactionRef = $"TXN-{order.OrderNumber}-{attemptNumber}";

        var payment = new Payment
        {
            UserId = userId,
            OrderId = orderId,
            PaymentMethodId = paymentMethodId,
            Amount = order.FinalAmount,
            Currency = PaymentCurrency.EGP,
            Status = PaymentStatus.Pending,
            TransactionRef = transactionRef,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var gatewayResult = await _paymentGateway.ProcessPaymentAsync(
            order.FinalAmount,
            "EGP",
            paymentMethodId);

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await _context.Database.BeginTransactionAsync();
            payment.TransactionRef = gatewayResult.TransactionRef;
            payment.GatewayResponse = gatewayResult.GatewayMessage;

            if (gatewayResult.Status == PaymentResultStatus.Succeeded)
            {
                payment.Status = PaymentStatus.Succeeded;
                payment.PaidAt = DateTime.UtcNow;
                order.Status = OrderStatus.Completed;

                foreach (var orderItem in order.OrderItems)
                {
                    await _enrollmentService.EnrollUserAsync(new CreateEnrollmentDto
                    {
                        UserId = userId,
                        CourseId = orderItem.CourseId,
                        Source = EnrollmentSource.Purchase
                    });
                }

                await _notificationService.CreateAndSendNotificationAsync(
                    userId,
                    "Payment Successful",
                    $"Your payment of {order.FinalAmount} EGP for order {order.OrderNumber} was successful.",
                    NotificationType.Payment,
                    $"/orders/{order.Id}");

                await _activityLogService.LogActivityAsync(
                    userId,
                    "Payment",
                    $"Payment of {order.FinalAmount} EGP for order {order.OrderNumber} completed successfully.",
                    "127.0.0.1");
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                order.Status = OrderStatus.Pending;

                await _notificationService.CreateAndSendNotificationAsync(
                    userId,
                    "Payment Failed",
                    $"Your payment of {order.FinalAmount} EGP for order {order.OrderNumber} failed. {gatewayResult.GatewayMessage}",
                    NotificationType.Payment,
                    $"/orders/{order.Id}");
            }

            await _context.SaveChangesAsync();
            if (transaction != null) await transaction.CommitAsync();
        }
        catch
        {
            if (transaction != null) await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            transaction?.Dispose();
        }

        return MapToDto(payment, paymentMethod.Name, attemptNumber);
    }

    public async Task<IEnumerable<PaymentResponseDto>> GetPaymentHistoryAsync(Guid orderId)
    {
        var payments = await _context.Payments
            .Include(p => p.PaymentMethod)
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return payments.Select((p, i) => MapToDto(p, p.PaymentMethod.Name, i + 1));
    }

    public async Task<PaymentMethodResponse> CreatePaymentMethodAsync(Guid adminUserId, CreatePaymentMethodRequest request)
    {
        var method = new PaymentMethod
        {
            Name = request.Name,
            Provider = request.Name,
            Type = PaymentMethodType.DigitalWallet,
            IsActive = true,
            Configuration = null
        };

        _context.PaymentMethods.Add(method);
        await _context.SaveChangesAsync();

        return new PaymentMethodResponse
        {
            Id = method.Id,
            Name = method.Name,
            Description = null,
            IconUrl = null,
            IsActive = method.IsActive,
            SortOrder = 0
        };
    }

    public async Task<bool> TogglePaymentMethodAsync(Guid paymentMethodId)
    {
        var method = await _context.PaymentMethods.FindAsync(paymentMethodId);
        if (method == null) return false;

        method.IsActive = !method.IsActive;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<PaymentMethodResponse>> GetActivePaymentMethodsAsync()
    {
        var methods = await _context.PaymentMethods
            .Where(pm => pm.IsActive)
            .ToListAsync();

        return methods.Select(m => new PaymentMethodResponse
        {
            Id = m.Id,
            Name = m.Name,
            Description = null,
            IconUrl = null,
            IsActive = m.IsActive,
            SortOrder = 0
        });
    }

    private static PaymentResponseDto MapToDto(Payment p, string methodName, int attemptNumber)
    {
        return new PaymentResponseDto
        {
            Id = p.Id,
            OrderId = p.OrderId,
            Amount = p.Amount,
            Status = p.Status.ToString(),
            GatewayTransactionId = p.TransactionRef,
            GatewayResponse = p.GatewayResponse,
            AttemptNumber = attemptNumber,
            PaymentMethodName = methodName,
            CreatedAt = p.CreatedAt
        };
    }
}
