using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.Refund;
using backend_project.DTOs.Enrollment;
using backend_project.Models;
using backend_project.Services.Interfaces;
using backend_project.Services.Notifications;

namespace backend_project.Services.Implementations;

public class RefundService : IRefundService
{
    private readonly ApplicationDbContext _context;
    private readonly IEnrollmentService _enrollmentService;
    private readonly INotificationService _notificationService;
    private readonly IActivityLogService _activityLogService;

    public RefundService(
        ApplicationDbContext context,
        IEnrollmentService enrollmentService,
        INotificationService notificationService,
        IActivityLogService activityLogService)
    {
        _context = context;
        _enrollmentService = enrollmentService;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
    }

    public async Task<RefundResponseDto> RequestRefundAsync(Guid userId, RequestRefundRequest request)
    {
        var payment = await _context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId && p.UserId == userId);

        if (payment == null)
            throw new KeyNotFoundException("Payment not found.");

        if (payment.Status != PaymentStatus.Succeeded)
            throw new InvalidOperationException("Can only request refund for a successful payment.");

        var existingRefund = await _context.Refunds
            .AnyAsync(r => r.PaymentId == request.PaymentId && r.Status == Models.RefundStatus.Requested);
        if (existingRefund)
            throw new InvalidOperationException("A refund request already exists for this payment.");

        var refund = new Refund
        {
            PaymentId = request.PaymentId,
            Amount = payment.Amount,
            Reason = request.Reason,
            Status = Models.RefundStatus.Requested,
            RequestedAt = DateTime.UtcNow
        };

        _context.Refunds.Add(refund);
        await _context.SaveChangesAsync();

        return MapToDto(refund, payment.Order?.OrderNumber);
    }

    public async Task<IEnumerable<RefundResponseDto>> GetUserRefundsAsync(Guid userId)
    {
        var refunds = await _context.Refunds
            .Include(r => r.Payment)
                .ThenInclude(p => p.Order)
            .Where(r => r.Payment.UserId == userId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

        return refunds.Select(r => MapToDto(r, r.Payment.Order?.OrderNumber));
    }

    public async Task<IEnumerable<RefundResponseDto>> GetAllRefundsAsync(Models.RefundStatus? status = null)
    {
        var query = _context.Refunds
            .Include(r => r.Payment)
                .ThenInclude(p => p.Order)
            .Include(r => r.ProcessedByUser)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        var refunds = await query.OrderByDescending(r => r.RequestedAt).ToListAsync();
        return refunds.Select(r => MapToDto(r, r.Payment.Order?.OrderNumber, r.ProcessedByUser));
    }

    public async Task<RefundResponseDto> ApproveRefundAsync(Guid adminId, ProcessRefundRequest request)
    {
        var refund = await _context.Refunds
            .Include(r => r.Payment)
                .ThenInclude(p => p.Order)
                    .ThenInclude(o => o.OrderItems)
            .Include(r => r.ProcessedByUser)
            .FirstOrDefaultAsync(r => r.Id == request.RefundId);

        if (refund == null)
            throw new KeyNotFoundException("Refund request not found.");

        if (refund.Status != Models.RefundStatus.Requested)
            throw new InvalidOperationException("Refund request is already processed.");

        refund.Status = Models.RefundStatus.Approved;
        refund.ProcessedAt = DateTime.UtcNow;
        refund.ProcessedBy = adminId;

        var order = refund.Payment.Order;
        order.Status = OrderStatus.Refunded;

        foreach (var orderItem in order.OrderItems)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == orderItem.CourseId && e.UserId == order.UserId);
            if (enrollment != null)
            {
                enrollment.Status = EnrollmentStatus.Refunded;
                enrollment.IsRefunded = true;

                if (enrollment.CertificateId.HasValue)
                {
                    var certificate = await _context.Certificates
                        .FirstOrDefaultAsync(c => c.Id == enrollment.CertificateId.Value);
                    if (certificate != null && certificate.Status == CertificateStatus.Valid)
                    {
                        certificate.Status = CertificateStatus.Revoked;
                        certificate.RevokedAt = DateTime.UtcNow;
                        certificate.RevokedBy = adminId;
                    }
                }
            }
        }

        await _context.SaveChangesAsync();

        await _activityLogService.LogActivityAsync(
            adminId,
            "RefundApproved",
            $"Refund of {refund.Amount} EGP for order {order.OrderNumber} approved.",
            "127.0.0.1");

        await _notificationService.CreateAndSendNotificationAsync(
            order.UserId,
            "Refund Approved",
            $"Your refund request for {refund.Amount} EGP has been approved.",
            NotificationType.Payment,
            $"/orders/{order.Id}");

        return MapToDto(refund, order.OrderNumber, refund.ProcessedByUser);
    }

    public async Task<RefundResponseDto> RejectRefundAsync(Guid adminId, ProcessRefundRequest request)
    {
        var refund = await _context.Refunds
            .Include(r => r.Payment)
                .ThenInclude(p => p.Order)
            .Include(r => r.ProcessedByUser)
            .FirstOrDefaultAsync(r => r.Id == request.RefundId);

        if (refund == null)
            throw new KeyNotFoundException("Refund request not found.");

        if (refund.Status != Models.RefundStatus.Requested)
            throw new InvalidOperationException("Refund request is already processed.");

        refund.Status = Models.RefundStatus.Rejected;
        refund.ProcessedAt = DateTime.UtcNow;
        refund.ProcessedBy = adminId;
        await _context.SaveChangesAsync();

        await _activityLogService.LogActivityAsync(
            adminId,
            "RefundRejected",
            $"Refund of {refund.Amount} EGP for order {refund.Payment.Order?.OrderNumber} rejected.",
            "127.0.0.1");

        await _notificationService.CreateAndSendNotificationAsync(
            refund.Payment.Order.UserId,
            "Refund Rejected",
            $"Your refund request for {refund.Amount} EGP has been rejected.",
            NotificationType.Payment);

        return MapToDto(refund, refund.Payment.Order?.OrderNumber, refund.ProcessedByUser);
    }

    private static RefundResponseDto MapToDto(Refund r, string? orderNumber, User? processedBy = null)
    {
        return new RefundResponseDto
        {
            Id = r.Id,
            PaymentId = r.PaymentId,
            Amount = r.Amount,
            Reason = r.Reason,
            Status = r.Status.ToString(),
            OrderNumber = orderNumber,
            RequestedAt = r.RequestedAt,
            ProcessedAt = r.ProcessedAt,
            ProcessedByName = processedBy != null ? $"{processedBy.FirstName} {processedBy.LastName}" : null
        };
    }
}
