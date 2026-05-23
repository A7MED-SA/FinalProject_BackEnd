using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.Cart;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _context;
    private readonly ICouponService _couponService;

    public CartService(ApplicationDbContext context, ICouponService couponService)
    {
        _context = context;
        _couponService = couponService;
    }

    public async Task<CartResponseDto> GetCartAsync(Guid userId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Course)
                    .ThenInclude(c => c.Creator)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Course)
                    .ThenInclude(c => c.CourseImageFile)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            return new CartResponseDto
            {
                Items = new List<CartItemDto>(),
                Subtotal = 0,
                FinalAmount = 0
            };
        }

        var items = cart.CartItems.Select(ci => new CartItemDto
        {
            Id = ci.Id,
            CourseId = ci.CourseId,
            CourseTitle = ci.Course.Title,
            CourseImageUrl = ci.Course.CourseImageFile?.Id.ToString(),
            InstructorName = $"{ci.Course.Creator.FirstName} {ci.Course.Creator.LastName}",
            PriceSnapshot = ci.PriceSnapshot,
            CurrentPrice = ci.Course.Price,
            AddedAt = ci.AddedAt
        }).ToList();

        var subtotal = items.Sum(i => i.PriceSnapshot);
        decimal discountAmount = 0;

        return new CartResponseDto
        {
            Id = cart.Id,
            Items = items,
            Subtotal = subtotal,
            FinalAmount = subtotal - discountAmount
        };
    }

    public async Task<CartItemDto> AddItemAsync(Guid userId, Guid courseId)
    {
        var course = await _context.Courses
            .Include(c => c.CourseImageFile)
            .Include(c => c.Creator)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            throw new KeyNotFoundException("Course not found.");

        if (course.Status != CourseStatus.Published)
            throw new InvalidOperationException("Cannot add an unpublished course to cart.");

        bool alreadyEnrolled = await _context.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
        if (alreadyEnrolled)
            throw new InvalidOperationException("You are already enrolled in this course.");

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        if (cart.CartItems.Any(ci => ci.CourseId == courseId))
            throw new InvalidOperationException("Course is already in your cart.");

        var cartItem = new CartItem
        {
            CartId = cart.Id,
            CourseId = courseId,
            PriceSnapshot = course.Price,
            AddedAt = DateTime.UtcNow
        };

        _context.CartItems.Add(cartItem);
        await _context.SaveChangesAsync();

        return new CartItemDto
        {
            Id = cartItem.Id,
            CourseId = courseId,
            CourseTitle = course.Title,
            CourseImageUrl = course.CourseImageFile?.Id.ToString(),
            InstructorName = $"{course.Creator.FirstName} {course.Creator.LastName}",
            PriceSnapshot = cartItem.PriceSnapshot,
            CurrentPrice = course.Price,
            AddedAt = cartItem.AddedAt
        };
    }

    public async Task RemoveItemAsync(Guid userId, Guid itemId)
    {
        var cartItem = await _context.CartItems
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci => ci.Id == itemId && ci.Cart.UserId == userId);

        if (cartItem == null)
            throw new KeyNotFoundException("Cart item not found.");

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();
    }

    public async Task<ApplyCouponResponse> ApplyCouponAsync(Guid userId, string code)
    {
        var cart = await GetCartAsync(userId);
        if (!cart.Items.Any())
            throw new InvalidOperationException("Cart is empty.");

        var courseIds = cart.Items.Select(i => i.CourseId).ToList();
        var result = await _couponService.ValidateAndApplyAsync(code, cart.Subtotal, courseIds, userId);

        return new ApplyCouponResponse
        {
            Code = code,
            DiscountAmount = result.DiscountAmount,
            FinalAmount = result.FinalAmount
        };
    }

    public async Task RemoveCouponAsync(Guid userId)
    {
        await Task.CompletedTask;
    }
}
