using backend_project.DTOs.Review;

namespace backend_project.Services.Interfaces;

public interface IReviewService
{
    Task<ReviewResponse> CreateReviewAsync(Guid userId, CreateReviewRequest request);
    Task<ReviewResponse> UpdateReviewAsync(Guid reviewId, Guid userId, CreateReviewRequest request);
    Task DeleteReviewAsync(Guid reviewId, Guid userId);
    Task<ReviewDetailResponse> GetReviewByIdAsync(Guid reviewId);
    Task<IEnumerable<ReviewResponse>> GetCourseReviewsAsync(Guid courseId, int page, int pageSize);
    Task<object> ToggleHelpfulAsync(Guid reviewId, Guid userId, ReviewHelpfulRequest request);
    Task FlagReviewAsync(Guid reviewId, Guid instructorId);
    Task<IEnumerable<ReviewDetailResponse>> GetPendingReviewsAsync();
    Task<ReviewResponse> ModerateReviewAsync(Guid reviewId, Guid moderatorId, ModerateReviewRequest request);
}
