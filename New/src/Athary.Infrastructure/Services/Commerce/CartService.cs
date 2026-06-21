using Athary.Application.DTOs.Commerce;
using Athary.Application.Interfaces.Commerce;
using Athary.Domain.Entities;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.Commerce;

public sealed class CartService : ICartService
{
    private readonly ApplicationDbContext _context;
    private readonly ICouponService _couponService;

    public CartService(ApplicationDbContext context, ICouponService couponService)
    {
        _context = context;
        _couponService = couponService;
    }

    public async Task<CartResponseDto> GetCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .AsNoTracking()
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Course)
                    .ThenInclude(c => c.Creator)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Course)
                    .ThenInclude(c => c.CourseImageFile)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart is null)
            return new CartResponseDto();

        var subtotal = cart.CartItems.Sum(ci => ci.PriceSnapshot);

        return MapCart(cart, subtotal, null, 0);
    }

    public async Task<CartItemDto> AddItemAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .Include(c => c.Creator)
            .Include(c => c.CourseImageFile)
            .FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken)
            ?? throw new KeyNotFoundException("الدورة غير موجودة.");

        var alreadyEnrolled = await _context.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == courseId, cancellationToken);

        if (alreadyEnrolled)
            throw new InvalidOperationException("أنت مسجل بالفعل في هذه الدورة.");

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart is null)
        {
            cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
        }

        var alreadyInCart = cart.CartItems.Any(ci => ci.CourseId == courseId);
        if (alreadyInCart)
            throw new InvalidOperationException("الدورة موجودة بالفعل في سلة التسوق.");

        var item = new CartItem
        {
            Cart = cart,
            CourseId = courseId,
            PriceSnapshot = course.Price,
            AddedAt = DateTime.UtcNow
        };

        cart.CartItems.Add(item);
        cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new CartItemDto
        {
            Id = item.Id,
            CourseId = courseId,
            CourseTitle = course.Title,
            CourseImageUrl = course.CourseImageFile is not null ? $"/api/files/{course.CourseImageFile.Id}/download" : null,
            InstructorName = course.Creator is not null ? $"{course.Creator.FirstName} {course.Creator.LastName}" : null,
            PriceSnapshot = course.Price,
            CurrentPrice = course.Price,
            AddedAt = item.AddedAt
        };
    }

    public async Task RemoveItemAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("سلة التسوق غير موجودة.");

        var item = cart.CartItems.FirstOrDefault(ci => ci.Id == itemId)
            ?? throw new KeyNotFoundException("العنصر غير موجود في سلة التسوق.");

        cart.CartItems.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ApplyCouponResponse> ApplyCouponAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .AsNoTracking()
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("سلة التسوق فارغة.");

        if (cart.CartItems.Count == 0)
            throw new InvalidOperationException("سلة التسوق فارغة.");

        var subtotal = cart.CartItems.Sum(ci => ci.PriceSnapshot);
        var courseIds = cart.CartItems.Select(ci => ci.CourseId).ToList();

        var result = await _couponService.ValidateCouponAsync(code, subtotal, courseIds, cancellationToken);

        return new ApplyCouponResponse
        {
            Code = result.Code,
            DiscountAmount = result.DiscountAmount,
            FinalAmount = result.FinalAmount,
            Message = result.Message
        };
    }

    public Task RemoveCouponAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    private static CartResponseDto MapCart(Cart? cart, decimal subtotal, string? couponCode, decimal discount)
    {
        if (cart is null)
            return new CartResponseDto();

        return new CartResponseDto
        {
            Id = cart.Id,
            Items = cart.CartItems.Select(ci => new CartItemDto
            {
                Id = ci.Id,
                CourseId = ci.CourseId,
                CourseTitle = ci.Course.Title,
                CourseImageUrl = ci.Course.CourseImageFile is not null ? $"/api/files/{ci.Course.CourseImageFile.Id}/download" : null,
                InstructorName = ci.Course.Creator is not null ? $"{ci.Course.Creator.FirstName} {ci.Course.Creator.LastName}" : null,
                PriceSnapshot = ci.PriceSnapshot,
                CurrentPrice = ci.Course.Price,
                AddedAt = ci.AddedAt
            }).ToList(),
            Subtotal = subtotal,
            CouponCode = couponCode,
            DiscountAmount = discount,
            FinalAmount = subtotal - discount
        };
    }
}
