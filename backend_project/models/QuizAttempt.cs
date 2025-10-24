using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("quiz_attempts")]
public class QuizAttempt : BaseEntity
{

    [Required]
    [Column("enrollment_id")]
    public Guid EnrollmentId { get; set; }

    [Required]
    [Column("quiz_id")]
    public Guid QuizId { get; set; }

    [Column("score")]
    public int Score { get; set; } = 0;

    [Column("max_score")]
    public int MaxScore { get; set; } = 0;

    [Column("status")]
    [MaxLength(20)]
    public QuizAttemptStatus Status { get; set; } = QuizAttemptStatus.InProgress;

    [Column("started_at")]
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    [Column("submitted_at")]
    public DateTime? SubmittedAt { get; set; }

    [Column("attempt_number")]
    public int AttemptNumber { get; set; } = 1;

    [Column("time_taken_seconds")]
    public int? TimeTakenSeconds { get; set; }

    // Navigation Properties
    [ForeignKey("EnrollmentId")]
    public virtual Enrollment Enrollment { get; set; } = null!;

    [ForeignKey("QuizId")]
    public virtual Quiz Quiz { get; set; } = null!;

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}

public enum QuizAttemptStatus
{
    InProgress,
    Submitted,
    Graded
}
