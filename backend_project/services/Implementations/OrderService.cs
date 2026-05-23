using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.Order;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IPaymentService _paymentService;
    private readonly ICouponService _couponService;
    private readonly IActivityLogService _activityLogService;

    public OrderService(
        ApplicationDbContext context,
        IPaymentService paymentService,
        ICouponService couponService,
        IActivityLogService activityLogService)
    {
        _context = context;
        _paymentService = paymentService;
        _couponService = couponService;
        _activityLogService = activityLogService;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(Guid userId, CreateOrderRequest request)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Course)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.CartItems.Any())
            throw new InvalidOperationException("Cart is empty.");

        var cartItems = cart.CartItems.ToList();
        var subtotal = cartItems.Sum(ci => ci.PriceSnapshot);
        decimal discountAmount = 0;
        Coupon? coupon = null;

        if (request.CouponId.HasValue)
        {
            coupon = await _context.Coupons.FindAsync(request.CouponId.Value);
            if (coupon == null || !coupon.IsActive)
                throw new InvalidOperationException("Invalid or inactive coupon.");

            var courseIds = cartItems.Select(ci => ci.CourseId).ToList();
            var validation = await _couponService.ValidateAndApplyAsync(
                coupon.Code, subtotal, courseIds, userId);

            if (!validation.IsValid)
                throw new InvalidOperationException(validation.Message ?? "Coupon validation failed.");

            discountAmount = validation.DiscountAmount;
        }

        var orderNumber = GenerateOrderNumber();
        var order = new Order
        {
            OrderNumber = orderNumber,
            UserId = userId,
            SubtotalAmount = subtotal,
            CouponId = request.CouponId,
            DiscountAmount = discountAmount,
            TaxAmount = 0,
            FinalAmount = subtotal - discountAmount,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Currency = "EGP"
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        foreach (var cartItem in cartItems)
        {
            _context.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                CourseId = cartItem.CourseId,
                PriceAtPurchase = cartItem.PriceSnapshot,
                AddedAt = DateTime.UtcNow
            });
        }

        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        if (coupon != null)
        {
            coupon.TimesUsed++;
            _context.CouponUsages.Add(new CouponUsage
            {
                CouponId = coupon.Id,
                UserId = userId,
                OrderId = order.Id,
                UsedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        await _activityLogService.LogActivityAsync(
            userId,
            "OrderCreated",
            $"Order {order.OrderNumber} created with {cartItems.Count} items for {order.FinalAmount} EGP.",
            "127.0.0.1");

        return MapToDto(order, cartItems.Count);
    }

    public async Task<IEnumerable<OrderResponseDto>> GetOrdersAsync(Guid userId)
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(o => new OrderResponseDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            Subtotal = o.SubtotalAmount,
            DiscountAmount = o.DiscountAmount,
            FinalAmount = o.FinalAmount,
            Status = o.Status.ToString(),
            ItemCount = o.OrderItems.Count,
            CreatedAt = o.CreatedAt
        });
    }

    public async Task<OrderDetailDto> GetOrderDetailsAsync(Guid userId, Guid orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Course)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
            throw new KeyNotFoundException("Order not found.");

        return new OrderDetailDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Subtotal = order.SubtotalAmount,
            DiscountAmount = order.DiscountAmount,
            FinalAmount = order.FinalAmount,
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                CourseId = oi.CourseId,
                CourseTitle = oi.Course.Title,
                PriceAtPurchase = oi.PriceAtPurchase
            }).ToList(),
            Payments = order.Payments.Select(p => new DTOs.Order.PaymentHistoryDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Status = p.Status.ToString(),
                GatewayResponse = p.GatewayResponse,
                AttemptNumber = 0,
                CreatedAt = p.CreatedAt
            }).ToList()
        };
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().ToUpper()[..6]}";
    }

    private static OrderResponseDto MapToDto(Order order, int itemCount)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Subtotal = order.SubtotalAmount,
            DiscountAmount = order.DiscountAmount,
            FinalAmount = order.FinalAmount,
            Status = order.Status.ToString(),
            ItemCount = itemCount,
            CreatedAt = order.CreatedAt
        };
    }
}
