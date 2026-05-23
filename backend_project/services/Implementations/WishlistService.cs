using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.Wishlist;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class WishlistService : IWishlistService
{
    private readonly ApplicationDbContext _context;

    public WishlistService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WishlistResponseDto> GetWishlistAsync(Guid userId)
    {
        var items = await _context.Wishlists
            .Include(w => w.Course)
                .ThenInclude(c => c.Creator)
            .Include(w => w.Course)
                .ThenInclude(c => c.CourseImageFile)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.AddedAt)
            .ToListAsync();

        return new WishlistResponseDto
        {
            Count = items.Count,
            Items = items.Select(w => new WishlistItemDto
            {
                Id = w.Id,
                CourseId = w.CourseId,
                CourseTitle = w.Course.Title,
                CourseImageUrl = w.Course.CourseImageFile?.Id.ToString(),
                InstructorName = $"{w.Course.Creator.FirstName} {w.Course.Creator.LastName}",
                Price = w.Course.Price,
                AddedAt = w.AddedAt
            }).ToList()
        };
    }

    public async Task<WishlistItemDto> AddItemAsync(Guid userId, Guid courseId)
    {
        var course = await _context.Courses
            .Include(c => c.CourseImageFile)
            .Include(c => c.Creator)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            throw new KeyNotFoundException("Course not found.");

        bool exists = await _context.Wishlists
            .AnyAsync(w => w.UserId == userId && w.CourseId == courseId);
        if (exists)
            throw new InvalidOperationException("Course is already in your wishlist.");

        var wishlist = new Wishlist
        {
            UserId = userId,
            CourseId = courseId,
            AddedAt = DateTime.UtcNow
        };

        _context.Wishlists.Add(wishlist);
        await _context.SaveChangesAsync();

        return new WishlistItemDto
        {
            Id = wishlist.Id,
            CourseId = courseId,
            CourseTitle = course.Title,
            CourseImageUrl = course.CourseImageFile?.Id.ToString(),
            InstructorName = $"{course.Creator.FirstName} {course.Creator.LastName}",
            Price = course.Price,
            AddedAt = wishlist.AddedAt
        };
    }

    public async Task RemoveItemAsync(Guid userId, Guid courseId)
    {
        var item = await _context.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.CourseId == courseId);

        if (item == null)
            throw new KeyNotFoundException("Wishlist item not found.");

        _context.Wishlists.Remove(item);
        await _context.SaveChangesAsync();
    }
}
