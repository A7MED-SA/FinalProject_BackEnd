using Athary.Application.DTOs.Wishlist;
using Athary.Application.Interfaces.Wishlist;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.Wishlist;

public sealed class WishlistService : IWishlistService
{
    private readonly ApplicationDbContext _context;

    public WishlistService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WishlistResponseDto> GetWishlistAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var items = await _context.Wishlists
            .AsNoTracking()
            .Include(w => w.Course)
                .ThenInclude(c => c.Creator)
            .Include(w => w.Course.CourseImageFile)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.AddedAt)
            .ToListAsync(cancellationToken);

        return new WishlistResponseDto
        {
            Items = items.Select(MapToDto).ToList(),
            Count = items.Count
        };
    }

    public async Task<WishlistItemDto> AddItemAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .Include(c => c.Creator)
            .Include(c => c.CourseImageFile)
            .FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken);

        if (course == null)
            throw new KeyNotFoundException("Course not found.");

        var exists = await _context.Wishlists
            .AnyAsync(w => w.UserId == userId && w.CourseId == courseId, cancellationToken);

        if (exists)
            throw new InvalidOperationException("Course is already in your wishlist.");

        var wishlist = new Athary.Domain.Entities.Wishlist
        {
            UserId = userId,
            CourseId = courseId,
            AddedAt = DateTime.UtcNow
        };

        _context.Wishlists.Add(wishlist);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(wishlist);
    }

    public async Task RemoveItemAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default)
    {
        var item = await _context.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.CourseId == courseId, cancellationToken);

        if (item == null)
            throw new KeyNotFoundException("Item not found in wishlist.");

        _context.Wishlists.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static WishlistItemDto MapToDto(Athary.Domain.Entities.Wishlist wishlist)
    {
        return new WishlistItemDto
        {
            Id = wishlist.Id,
            CourseId = wishlist.CourseId,
            CourseTitle = wishlist.Course.Title,
            CourseImageUrl = wishlist.Course.CourseImageFile != null
                ? $"/api/files/{wishlist.Course.CourseImageFile.Id}/download"
                : null,
            InstructorName = wishlist.Course.Creator != null
                ? $"{wishlist.Course.Creator.FirstName} {wishlist.Course.Creator.LastName}"
                : null,
            Price = wishlist.Course.Price,
            AddedAt = wishlist.AddedAt
        };
    }
}
