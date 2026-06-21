using Athary.Application.DTOs.Review;

namespace Athary.Application.Interfaces.Review;

public interface IReviewService
{
    Task<ReviewResponse> CreateReviewAsync(Guid userId, CreateReviewRequest request, CancellationToken cancellationToken = default);
    Task<ReviewResponse> UpdateReviewAsync(Guid reviewId, Guid userId, CreateReviewRequest request, CancellationToken cancellationToken = default);
    Task DeleteReviewAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default);
    Task<ReviewDetailResponse> GetReviewByIdAsync(Guid reviewId, CancellationToken cancellationToken = default);
    Task<List<ReviewResponse>> GetCourseReviewsAsync(Guid courseId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task ToggleHelpfulAsync(Guid reviewId, Guid userId, ReviewHelpfulRequest request, CancellationToken cancellationToken = default);
    Task FlagReviewAsync(Guid reviewId, Guid instructorId, CancellationToken cancellationToken = default);
    Task<List<ReviewDetailResponse>> GetPendingReviewsAsync(CancellationToken cancellationToken = default);
    Task<ReviewResponse> ModerateReviewAsync(Guid reviewId, Guid moderatorId, ModerateReviewRequest request, CancellationToken cancellationToken = default);
}
