using Athary.Application.DTOs.Commerce;
using Athary.Application.Interfaces.Authentication;
using Athary.Application.Interfaces.Commerce;
using Athary.Application.Interfaces.Notification;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.Commerce;

public sealed class RefundService : IRefundService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IActivityLogService _activityLogService;

    public RefundService(
        ApplicationDbContext context,
        INotificationService notificationService,
        IActivityLogService activityLogService)
    {
        _context = context;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
    }

    public async Task<RefundResponseDto> RequestRefundAsync(Guid userId, RequestRefundRequest request, CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId && p.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("عملية الدفع غير موجودة.");

        if (payment.Status != PaymentStatus.Succeeded)
            throw new InvalidOperationException("لا يمكن طلب استرداد لعملية دفع لم تتم بنجاح.");

        if (payment.Order.Status != OrderStatus.Completed)
            throw new InvalidOperationException("لا يمكن طلب استرداد لطلب غير مكتمل.");

        var existingRefund = await _context.Refunds
            .AnyAsync(r => r.PaymentId == request.PaymentId && r.Status == RefundStatus.Requested, cancellationToken);

        if (existingRefund)
            throw new InvalidOperationException("يوجد طلب استرداد قيد المراجعة لهذه العملية.");

        var refund = new Refund
        {
            PaymentId = request.PaymentId,
            Amount = payment.Amount,
            Reason = request.Reason,
            Status = RefundStatus.Requested,
            RequestedAt = DateTime.UtcNow
        };

        _context.Refunds.Add(refund);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(refund, payment.Order.OrderNumber);
    }

    public async Task<IEnumerable<RefundResponseDto>> GetUserRefundsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Refunds
            .AsNoTracking()
            .Include(r => r.Payment)
                .ThenInclude(p => p.Order)
            .Include(r => r.ProcessedByUser)
            .Where(r => r.Payment.UserId == userId)
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => new RefundResponseDto
            {
                Id = r.Id,
                PaymentId = r.PaymentId,
                Amount = r.Amount,
                Reason = r.Reason,
                Status = r.Status.ToString(),
                OrderNumber = r.Payment.Order.OrderNumber,
                RequestedAt = r.RequestedAt,
                ProcessedAt = r.ProcessedAt,
                ProcessedByName = r.ProcessedByUser != null
                    ? r.ProcessedByUser.FirstName + " " + r.ProcessedByUser.LastName
                    : null
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RefundResponseDto>> GetAllRefundsAsync(RefundStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Refunds
            .AsNoTracking()
            .Include(r => r.Payment)
                .ThenInclude(p => p.Order)
            .Include(r => r.ProcessedByUser)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        return await query
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => new RefundResponseDto
            {
                Id = r.Id,
                PaymentId = r.PaymentId,
                Amount = r.Amount,
                Reason = r.Reason,
                Status = r.Status.ToString(),
                OrderNumber = r.Payment.Order.OrderNumber,
                RequestedAt = r.RequestedAt,
                ProcessedAt = r.ProcessedAt,
                ProcessedByName = r.ProcessedByUser != null
                    ? r.ProcessedByUser.FirstName + " " + r.ProcessedByUser.LastName
                    : null
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<RefundResponseDto> ApproveRefundAsync(Guid adminId, ProcessRefundRequest request, CancellationToken cancellationToken = default)
    {
        var refund = await _context.Refunds
            .Include(r => r.Payment)
                .ThenInclude(p => p.Order)
                    .ThenInclude(o => o.OrderItems)
            .FirstOrDefaultAsync(r => r.Id == request.RefundId, cancellationToken)
            ?? throw new KeyNotFoundException("طلب الاسترداد غير موجود.");

        if (refund.Status != RefundStatus.Requested)
            throw new InvalidOperationException("يمكن الموافقة فقط على طلبات الاسترداد قيد المراجعة.");

        refund.Status = RefundStatus.Approved;
        refund.ProcessedAt = DateTime.UtcNow;
        refund.ProcessedBy = adminId;

        refund.Payment.Status = PaymentStatus.Pending;
        refund.Payment.Order.Status = OrderStatus.Refunded;
        refund.Payment.Order.UpdatedAt = DateTime.UtcNow;

        foreach (var item in refund.Payment.Order.OrderItems)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == refund.Payment.UserId && e.CourseId == item.CourseId, cancellationToken);

            if (enrollment is not null)
            {
                enrollment.Status = EnrollmentStatus.Refunded;
            }
        }

        await _notificationService.CreateAndSendNotificationAsync(
            refund.Payment.UserId,
            "تمت الموافقة على طلب الاسترداد",
            $"تمت الموافقة على طلب استرداد مبلغ {refund.Amount} للطلب #{refund.Payment.Order.OrderNumber}.",
            NotificationType.Payment,
            cancellationToken: cancellationToken);

        await _activityLogService.LogActivityAsync(
            adminId, "ApproveRefund", $"Approved refund {refund.Id} for order {refund.Payment.Order.OrderNumber}",
            "system", cancellationToken: cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        refund.Status = RefundStatus.Processed;
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(refund, refund.Payment.Order.OrderNumber);
    }

    public async Task<RefundResponseDto> RejectRefundAsync(Guid adminId, ProcessRefundRequest request, CancellationToken cancellationToken = default)
    {
        var refund = await _context.Refunds
            .Include(r => r.Payment)
                .ThenInclude(p => p.Order)
            .FirstOrDefaultAsync(r => r.Id == request.RefundId, cancellationToken)
            ?? throw new KeyNotFoundException("طلب الاسترداد غير موجود.");

        if (refund.Status != RefundStatus.Requested)
            throw new InvalidOperationException("يمكن رفض فقط طلبات الاسترداد قيد المراجعة.");

        refund.Status = RefundStatus.Rejected;
        refund.ProcessedAt = DateTime.UtcNow;
        refund.ProcessedBy = adminId;

        await _notificationService.CreateAndSendNotificationAsync(
            refund.Payment.UserId,
            "تم رفض طلب الاسترداد",
            $"عذراً، تم رفض طلب استرداد مبلغ {refund.Amount} للطلب #{refund.Payment.Order.OrderNumber}. السبب: {request.AdminNotes}",
            NotificationType.Payment,
            cancellationToken: cancellationToken);

        await _activityLogService.LogActivityAsync(
            adminId, "RejectRefund", $"Rejected refund {refund.Id} for order {refund.Payment.Order.OrderNumber}",
            "system", cancellationToken: cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(refund, refund.Payment.Order.OrderNumber);
    }

    private static RefundResponseDto MapToDto(Refund refund, string orderNumber) => new()
    {
        Id = refund.Id,
        PaymentId = refund.PaymentId,
        Amount = refund.Amount,
        Reason = refund.Reason,
        Status = refund.Status.ToString(),
        OrderNumber = orderNumber,
        RequestedAt = refund.RequestedAt,
        ProcessedAt = refund.ProcessedAt
    };
}
