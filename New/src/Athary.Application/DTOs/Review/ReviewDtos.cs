namespace Athary.Application.DTOs.Review;

public class CreateReviewRequest
{
    public Guid CourseId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class ReviewResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    public bool IsFlagged { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ReviewDetailResponse : ReviewResponse
{
    public DateTime? ModeratedAt { get; set; }
    public Guid? ModeratedBy { get; set; }
    public string? ModeratorName { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? FlaggedBy { get; set; }
    public DateTime? FlaggedAt { get; set; }
}

public class ReviewHelpfulRequest
{
    public bool IsHelpful { get; set; }
}

public class ModerateReviewRequest
{
    public string Status { get; set; } = string.Empty;
}
