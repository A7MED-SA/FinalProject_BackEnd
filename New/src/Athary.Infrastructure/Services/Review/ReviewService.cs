using Athary.Application.DTOs.Review;
using Athary.Application.Interfaces.Authentication;
using Athary.Application.Interfaces.Notification;
using Athary.Application.Interfaces.Review;
using Athary.Domain.Enums;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.Review;

public sealed class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IActivityLogService _activityLogService;

    public ReviewService(
        ApplicationDbContext context,
        INotificationService notificationService,
        IActivityLogService activityLogService)
    {
        _context = context;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
    }

    public async Task<ReviewResponse> CreateReviewAsync(Guid userId, CreateReviewRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Reviews
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == request.CourseId, cancellationToken);

        if (existing != null)
            throw new InvalidOperationException("You have already reviewed this course.");

        var course = await _context.Courses.FindAsync(new object[] { request.CourseId }, cancellationToken);
        if (course == null)
            throw new KeyNotFoundException("Course not found.");

        var isVerified = await _context.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == request.CourseId, cancellationToken);

        var review = new Athary.Domain.Entities.Review
        {
            UserId = userId,
            CourseId = request.CourseId,
            Rating = request.Rating,
            Comment = request.Comment,
            Status = ReviewStatus.Pending,
            IsVerified = isVerified,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync(cancellationToken);

        await _activityLogService.LogActivityAsync(
            userId, "ReviewCreated",
            $"Created a review for course '{course.Title}'",
            string.Empty,
            cancellationToken: cancellationToken);

        return await GetReviewDtoAsync(review.Id, cancellationToken);
    }

    public async Task<ReviewResponse> UpdateReviewAsync(Guid reviewId, Guid userId, CreateReviewRequest request, CancellationToken cancellationToken = default)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId, cancellationToken);

        if (review == null)
            throw new KeyNotFoundException("Review not found.");

        review.Rating = request.Rating;
        review.Comment = request.Comment;
        review.Status = ReviewStatus.Pending;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetReviewDtoAsync(review.Id, cancellationToken);
    }

    public async Task DeleteReviewAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId, cancellationToken);

        if (review == null)
            throw new KeyNotFoundException("Review not found.");

        review.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ReviewDetailResponse> GetReviewByIdAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        var review = await _context.Reviews
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(r => r.User)
            .Include(r => r.Course)
            .Include(r => r.Moderator)
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);

        if (review == null)
            throw new KeyNotFoundException("Review not found.");

        return MapToDetailDto(review);
    }

    public async Task<List<ReviewResponse>> GetCourseReviewsAsync(Guid courseId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var query = _context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Course)
            .Where(r => r.CourseId == courseId && r.Status == ReviewStatus.Approved && r.DeletedAt == null)
            .OrderByDescending(r => r.CreatedAt);

        var reviews = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return reviews.Select(MapToDto).ToList();
    }

    public async Task ToggleHelpfulAsync(Guid reviewId, Guid userId, ReviewHelpfulRequest request, CancellationToken cancellationToken = default)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);

        if (review == null)
            throw new KeyNotFoundException("Review not found.");

        var existing = await _context.ReviewHelpfuls
            .FirstOrDefaultAsync(rh => rh.ReviewId == reviewId && rh.UserId == userId, cancellationToken);

        if (existing != null)
        {
            if (existing.IsHelpful == request.IsHelpful)
            {
                _context.ReviewHelpfuls.Remove(existing);
                AdjustHelpfulCounts(review, existing.IsHelpful, true);
            }
            else
            {
                existing.IsHelpful = request.IsHelpful;
                AdjustHelpfulCounts(review, !request.IsHelpful, false);
                AdjustHelpfulCounts(review, request.IsHelpful, true);
            }
        }
        else
        {
            _context.ReviewHelpfuls.Add(new Athary.Domain.Entities.ReviewHelpful
            {
                ReviewId = reviewId,
                UserId = userId,
                IsHelpful = request.IsHelpful,
                CreatedAt = DateTime.UtcNow
            });
            AdjustHelpfulCounts(review, request.IsHelpful, true);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task FlagReviewAsync(Guid reviewId, Guid instructorId, CancellationToken cancellationToken = default)
    {
        var review = await _context.Reviews
            .Include(r => r.Course)
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);

        if (review == null)
            throw new KeyNotFoundException("Review not found.");

        if (review.Course.CreatedBy != instructorId)
            throw new UnauthorizedAccessException("You can only flag reviews on your own courses.");

        review.IsFlagged = true;
        review.FlaggedBy = instructorId;
        review.FlaggedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _activityLogService.LogActivityAsync(
            instructorId, "ReviewFlagged",
            $"Flagged review on course '{review.Course.Title}'",
            string.Empty,
            cancellationToken: cancellationToken);
    }

    public async Task<List<ReviewDetailResponse>> GetPendingReviewsAsync(CancellationToken cancellationToken = default)
    {
        var reviews = await _context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Course)
            .Include(r => r.Moderator)
            .Where(r => r.Status == ReviewStatus.Pending || r.IsFlagged)
            .OrderByDescending(r => r.IsFlagged)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return reviews.Select(MapToDetailDto).ToList();
    }

    public async Task<ReviewResponse> ModerateReviewAsync(Guid reviewId, Guid moderatorId, ModerateReviewRequest request, CancellationToken cancellationToken = default)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Course)
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);

        if (review == null)
            throw new KeyNotFoundException("Review not found.");

        if (!Enum.TryParse<ReviewStatus>(request.Status, true, out var status))
            throw new InvalidOperationException($"Invalid status: {request.Status}");

        review.Status = status;
        review.ModeratedAt = DateTime.UtcNow;
        review.ModeratedBy = moderatorId;

        if (status == ReviewStatus.Approved)
        {
            review.IsFlagged = false;
            review.FlaggedBy = null;
            review.FlaggedAt = null;
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.CreateAndSendNotificationAsync(
            review.UserId,
            "Review Moderated",
            $"Your review for course '{review.Course.Title}' has been {status.ToString().ToLowerInvariant()}.",
            NotificationType.Course,
            $"/courses/{review.CourseId}",
            cancellationToken: cancellationToken);

        return MapToDto(review);
    }

    private static void AdjustHelpfulCounts(Athary.Domain.Entities.Review review, bool isHelpful, bool increment)
    {
        var delta = increment ? 1 : -1;
        if (isHelpful)
            review.HelpfulCount += delta;
        else
            review.NotHelpfulCount += delta;
    }

    private async Task<ReviewResponse> GetReviewDtoAsync(Guid reviewId, CancellationToken cancellationToken)
    {
        var review = await _context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Course)
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);

        return MapToDto(review!);
    }

    private static ReviewResponse MapToDto(Athary.Domain.Entities.Review review)
    {
        return new ReviewResponse
        {
            Id = review.Id,
            UserId = review.UserId,
            UserFullName = $"{review.User.FirstName} {review.User.LastName}",
            CourseId = review.CourseId,
            CourseTitle = review.Course.Title,
            Rating = review.Rating,
            Comment = review.Comment,
            Status = review.Status.ToString(),
            IsVerified = review.IsVerified,
            HelpfulCount = review.HelpfulCount,
            NotHelpfulCount = review.NotHelpfulCount,
            IsFlagged = review.IsFlagged,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }

    private static ReviewDetailResponse MapToDetailDto(Athary.Domain.Entities.Review review)
    {
        return new ReviewDetailResponse
        {
            Id = review.Id,
            UserId = review.UserId,
            UserFullName = $"{review.User.FirstName} {review.User.LastName}",
            CourseId = review.CourseId,
            CourseTitle = review.Course.Title,
            Rating = review.Rating,
            Comment = review.Comment,
            Status = review.Status.ToString(),
            IsVerified = review.IsVerified,
            HelpfulCount = review.HelpfulCount,
            NotHelpfulCount = review.NotHelpfulCount,
            IsFlagged = review.IsFlagged,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt,
            ModeratedAt = review.ModeratedAt,
            ModeratedBy = review.ModeratedBy,
            ModeratorName = review.Moderator != null ? $"{review.Moderator.FirstName} {review.Moderator.LastName}" : null,
            DeletedAt = review.DeletedAt,
            FlaggedBy = review.FlaggedBy,
            FlaggedAt = review.FlaggedAt
        };
    }
}
