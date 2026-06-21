using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.Review;
using backend_project.Models;
using backend_project.Services.Interfaces;
using backend_project.Services.Notifications;

namespace backend_project.Services.Implementations;

public class ReviewService : IReviewService
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

    public async Task<ReviewResponse> CreateReviewAsync(Guid userId, CreateReviewRequest request)
    {
        var existing = await _context.Reviews
            .IgnoreQueryFilters()
            .AnyAsync(r => r.UserId == userId && r.CourseId == request.CourseId && r.DeletedAt == null);

        if (existing)
            throw new InvalidOperationException("You have already reviewed this course.");

        var isVerified = await _context.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == request.CourseId && e.Status == EnrollmentStatus.Completed);

        if (request.CourseId == Guid.Empty)
            throw new ArgumentException("CourseId is required.");

        var review = new Review
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
        await _context.SaveChangesAsync();

        return await MapToResponseDto(review.Id);
    }

    public async Task<ReviewResponse> UpdateReviewAsync(Guid reviewId, Guid userId, CreateReviewRequest request)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId && r.DeletedAt == null);

        if (review == null)
            throw new KeyNotFoundException("Review not found.");

        review.Rating = request.Rating;
        review.Comment = request.Comment;
        review.Status = ReviewStatus.Pending;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await MapToResponseDto(review.Id);
    }

    public async Task DeleteReviewAsync(Guid reviewId, Guid userId)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId && r.DeletedAt == null);

        if (review == null)
            throw new KeyNotFoundException("Review not found.");

        review.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<ReviewDetailResponse> GetReviewByIdAsync(Guid reviewId)
    {
        var review = await _context.Reviews
            .IgnoreQueryFilters()
            .Include(r => r.User)
            .Include(r => r.Course)
            .Include(r => r.Moderator)
            .Include(r => r.FlaggedByUser)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
            throw new KeyNotFoundException("Review not found.");

        return MapToDetailDto(review);
    }

    public async Task<IEnumerable<ReviewResponse>> GetCourseReviewsAsync(Guid courseId, int page, int pageSize)
    {
        var reviews = await _context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Course)
            .Where(r => r.CourseId == courseId && r.Status == ReviewStatus.Approved && r.DeletedAt == null)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return reviews.Select(MapToDto);
    }

    public async Task<object> ToggleHelpfulAsync(Guid reviewId, Guid userId, ReviewHelpfulRequest request)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.DeletedAt == null);

        if (review == null)
            throw new KeyNotFoundException("Review not found.");

        var existing = await _context.ReviewHelpfuls
            .FirstOrDefaultAsync(rh => rh.ReviewId == reviewId && rh.UserId == userId);

        if (existing != null)
        {
            if (existing.IsHelpful == request.IsHelpful)
            {
                _context.ReviewHelpfuls.Remove(existing);
                AdjustHelpfulCounts(review, existing.IsHelpful, -1);
                await _context.SaveChangesAsync();
                return new { Action = "removed" };
            }

            AdjustHelpfulCounts(review, existing.IsHelpful, -1);
            AdjustHelpfulCounts(review, request.IsHelpful, 1);
            existing.IsHelpful = request.IsHelpful;
            await _context.SaveChangesAsync();
            return new { Action = "updated", IsHelpful = request.IsHelpful };
        }

        _context.ReviewHelpfuls.Add(new ReviewHelpful
        {
            ReviewId = reviewId,
            UserId = userId,
            IsHelpful = request.IsHelpful
        });

        AdjustHelpfulCounts(review, request.IsHelpful, 1);
        await _context.SaveChangesAsync();
        return new { Action = "added", IsHelpful = request.IsHelpful };
    }

    public async Task FlagReviewAsync(Guid reviewId, Guid instructorId)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.DeletedAt == null);

        if (review == null)
            throw new KeyNotFoundException("Review not found.");

        var isInstructor = await _context.Courses
            .AnyAsync(c => c.Id == review.CourseId && c.CreatedBy == instructorId);

        if (!isInstructor)
            throw new InvalidOperationException("Only the course instructor can flag reviews.");

        review.IsFlagged = true;
        review.FlaggedBy = instructorId;
        review.FlaggedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _activityLogService.LogActivityAsync(
            instructorId,
            "ReviewFlagged",
            $"Review on course '{review.Course.Title}' flagged for moderation",
            "127.0.0.1");
    }

    public async Task<IEnumerable<ReviewDetailResponse>> GetPendingReviewsAsync()
    {
        var reviews = await _context.Reviews
            .IgnoreQueryFilters()
            .Include(r => r.User)
            .Include(r => r.Course)
            .Include(r => r.Moderator)
            .Include(r => r.FlaggedByUser)
            .Where(r => r.Status == ReviewStatus.Pending || r.IsFlagged)
            .OrderByDescending(r => r.IsFlagged)
            .ThenBy(r => r.CreatedAt)
            .ToListAsync();

        return reviews.Select(MapToDetailDto);
    }

    public async Task<ReviewResponse> ModerateReviewAsync(Guid reviewId, Guid moderatorId, ModerateReviewRequest request)
    {
        var review = await _context.Reviews
            .IgnoreQueryFilters()
            .Include(r => r.Course)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
            throw new KeyNotFoundException("Review not found.");

        var parsed = Enum.Parse<ReviewStatus>(request.Status, true);

        review.Status = parsed;
        review.ModeratedBy = moderatorId;
        review.ModeratedAt = DateTime.UtcNow;

        if (parsed == ReviewStatus.Approved)
        {
            review.IsFlagged = false;
            review.FlaggedBy = null;
            review.FlaggedAt = null;
        }

        await _context.SaveChangesAsync();

        await _activityLogService.LogActivityAsync(
            moderatorId,
            "ReviewModerated",
            $"Review on course '{review.Course.Title}' moderated to {parsed}",
            "127.0.0.1");

        var user = await _context.Users.FindAsync(review.UserId);
        if (user != null)
        {
            var statusMsg = parsed == ReviewStatus.Approved ? "approved" : "rejected";
            await _notificationService.CreateAndSendNotificationAsync(
                review.UserId,
                "Review Moderated",
                $"Your review for '{review.Course.Title}' has been {statusMsg}.",
                NotificationType.System);
        }

        return await MapToResponseDto(review.Id);
    }

    private async Task<ReviewResponse> MapToResponseDto(Guid reviewId)
    {
        var review = await _context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Course)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        return MapToDto(review!);
    }

    private static ReviewResponse MapToDto(Review review)
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

    private static ReviewDetailResponse MapToDetailDto(Review review)
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

    private static void AdjustHelpfulCounts(Review review, bool isHelpful, int delta)
    {
        if (isHelpful)
            review.HelpfulCount += delta;
        else
            review.NotHelpfulCount += delta;
    }
}
