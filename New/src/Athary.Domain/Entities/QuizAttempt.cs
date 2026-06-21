using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class QuizAttempt : BaseEntity
{
    public Guid EnrollmentId { get; set; }
    public Guid QuizId { get; set; }
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public QuizAttemptStatus Status { get; set; } = QuizAttemptStatus.InProgress;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public int AttemptNumber { get; set; } = 1;
    public int? TimeTakenSeconds { get; set; }

    public Enrollment Enrollment { get; set; } = null!;
    public Quiz Quiz { get; set; } = null!;
    public List<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
