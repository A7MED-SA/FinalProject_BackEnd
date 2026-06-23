using Athary.Application.DTOs.Public;
using Athary.Application.Interfaces.Public;
using Athary.Domain.Entities;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Athary.Infrastructure.Services.Public;

public class TestimonialService : ITestimonialService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TestimonialService> _logger;

    public TestimonialService(
        ApplicationDbContext context,
        ILogger<TestimonialService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<TestimonialDto>> GetApprovedAsync(int page, int pageSize, int? minRating = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Testimonials
            .Where(t => t.IsApproved && !t.IsFlagged);

        if (minRating.HasValue)
            query = query.Where(t => t.Rating >= minRating.Value);

        return await query
            .OrderBy(t => t.DisplayOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TestimonialDto
            {
                Id = t.Id,
                Content = t.Content,
                Rating = t.Rating,
                UserName = t.User.FullName,
                UserAvatar = t.User.ProfileImageFile != null ? t.User.ProfileImageFile.FilePath : null,
                DisplayOrder = t.DisplayOrder,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TestimonialDto> CreateAsync(Guid userId, CreateTestimonialDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Testimonials
            .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);

        if (existing is not null)
        {
            existing.Content = dto.Content;
            existing.Rating = dto.Rating;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Testimonial updated for user {UserId}", userId);

            return MapToDto(existing);
        }

        var testimonial = new Testimonial
        {
            Content = dto.Content,
            Rating = dto.Rating,
            UserId = userId,
            IsApproved = false,
            DisplayOrder = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.Testimonials.Add(testimonial);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Testimonial created for user {UserId}", userId);

        return MapToDto(testimonial);
    }

    public async Task<TestimonialDto> UpdateAsync(Guid userId, CreateTestimonialDto dto, CancellationToken cancellationToken = default)
    {
        return await CreateAsync(userId, dto, cancellationToken);
    }

    public async Task<List<TestimonialDto>> GetAllAsync(bool? isApproved = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Testimonials.AsQueryable();

        if (isApproved.HasValue)
            query = query.Where(t => t.IsApproved == isApproved.Value);

        return await query
            .OrderBy(t => t.DisplayOrder)
            .Select(t => new TestimonialDto
            {
                Id = t.Id,
                Content = t.Content,
                Rating = t.Rating,
                UserName = t.User.FullName,
                UserAvatar = t.User.ProfileImageFile != null ? t.User.ProfileImageFile.FilePath : null,
                DisplayOrder = t.DisplayOrder,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TestimonialDto> UpdateAdminAsync(Guid id, UpdateTestimonialDto dto, CancellationToken cancellationToken = default)
    {
        var testimonial = await _context.Testimonials.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new KeyNotFoundException("Testimonial not found");

        if (dto.Content is not null)
            testimonial.Content = dto.Content;

        if (dto.Rating.HasValue)
            testimonial.Rating = dto.Rating.Value;

        if (dto.IsApproved.HasValue)
            testimonial.IsApproved = dto.IsApproved.Value;

        if (dto.DisplayOrder.HasValue)
            testimonial.DisplayOrder = dto.DisplayOrder.Value;

        testimonial.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Testimonial {Id} updated by admin", id);

        return MapToDto(testimonial);
    }

    public async Task ApproveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var testimonial = await _context.Testimonials.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new KeyNotFoundException("Testimonial not found");

        testimonial.IsApproved = true;
        testimonial.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Testimonial {Id} approved", id);
    }

    public async Task FlagAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        var testimonial = await _context.Testimonials.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new KeyNotFoundException("Testimonial not found");

        testimonial.IsFlagged = true;
        testimonial.FlagReason = reason;
        testimonial.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Testimonial {Id} flagged: {Reason}", id, reason);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var testimonial = await _context.Testimonials.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new KeyNotFoundException("Testimonial not found");

        _context.Testimonials.Remove(testimonial);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Testimonial {Id} deleted", id);
    }

    public async Task ReorderAsync(List<ReorderItemDto> items, CancellationToken cancellationToken = default)
    {
        foreach (var item in items)
        {
            var testimonial = await _context.Testimonials.FindAsync(new object[] { item.Id }, cancellationToken);
            if (testimonial is not null)
            {
                testimonial.DisplayOrder = item.DisplayOrder;
                testimonial.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Testimonials reordered");
    }

    private static TestimonialDto MapToDto(Testimonial testimonial)
    {
        return new TestimonialDto
        {
            Id = testimonial.Id,
            Content = testimonial.Content,
            Rating = testimonial.Rating,
            UserName = testimonial.User?.FullName ?? string.Empty,
            UserAvatar = testimonial.User?.ProfileImageFile?.FilePath,
            DisplayOrder = testimonial.DisplayOrder,
            CreatedAt = testimonial.CreatedAt
        };
    }
}
