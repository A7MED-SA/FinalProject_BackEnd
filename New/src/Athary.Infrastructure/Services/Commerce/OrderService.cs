using Athary.Application.DTOs.Commerce;
using Athary.Application.Interfaces.Commerce;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.Commerce;

public sealed class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ICouponService _couponService;
    private static long _orderCounter;

    public OrderService(ApplicationDbContext context, ICouponService couponService)
    {
        _context = context;
        _couponService = couponService;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(Guid userId, CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Course)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("سلة التسوق فارغة.");

        if (cart.CartItems.Count == 0)
            throw new InvalidOperationException("سلة التسوق فارغة.");

        var subtotal = cart.CartItems.Sum(ci => ci.PriceSnapshot);
        decimal discount = 0;

        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var courseIds = cart.CartItems.Select(ci => ci.CourseId).ToList();
            var result = await _couponService.ValidateAndApplyAsync(
                request.CouponCode, subtotal, courseIds, userId, cancellationToken);

            if (!result.IsValid)
                throw new InvalidOperationException(result.Message ?? "الكوبون غير صالح.");

            discount = result.DiscountAmount;
        }

        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Interlocked.Increment(ref _orderCounter):D6}";

        var order = new Order
        {
            UserId = userId,
            OrderNumber = orderNumber,
            SubtotalAmount = subtotal,
            DiscountAmount = discount,
            FinalAmount = subtotal - discount,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var cartItem in cart.CartItems)
        {
            order.OrderItems.Add(new OrderItem
            {
                CourseId = cartItem.CourseId,
                PriceAtPurchase = cartItem.PriceSnapshot
            });
        }

        _context.Orders.Add(order);
        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync(cancellationToken);

        return new OrderResponseDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Subtotal = order.SubtotalAmount,
            DiscountAmount = order.DiscountAmount,
            FinalAmount = order.FinalAmount,
            Status = order.Status.ToString(),
            ItemCount = order.OrderItems.Count,
            CreatedAt = order.CreatedAt
        };
    }

    public async Task<IEnumerable<OrderResponseDto>> GetOrdersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderResponseDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Subtotal = o.SubtotalAmount,
                DiscountAmount = o.DiscountAmount,
                FinalAmount = o.FinalAmount,
                Status = o.Status.ToString(),
                CouponCode = o.Coupon != null ? o.Coupon.Code : null,
                ItemCount = o.OrderItems.Count,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderDetailDto> GetOrderDetailsAsync(Guid userId, Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Course)
            .Include(o => o.Payments)
                .ThenInclude(p => p.PaymentMethod)
            .Include(o => o.Coupon)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("الطلب غير موجود.");

        return new OrderDetailDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Subtotal = order.SubtotalAmount,
            DiscountAmount = order.DiscountAmount,
            FinalAmount = order.FinalAmount,
            Status = order.Status.ToString(),
            CouponCode = order.Coupon?.Code,
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                CourseId = oi.CourseId,
                CourseTitle = oi.Course.Title,
                PriceAtPurchase = oi.PriceAtPurchase
            }).ToList(),
            Payments = order.Payments.Select(p => new PaymentHistoryDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Status = p.Status.ToString(),
                GatewayResponse = p.GatewayResponse,
                CreatedAt = p.CreatedAt
            }).ToList(),
            CreatedAt = order.CreatedAt
        };
    }
}
