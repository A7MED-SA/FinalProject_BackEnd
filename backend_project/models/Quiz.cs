using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("quizzes")]
public class Quiz : BaseEntity
{

    [Required]
    [Column("title")]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("duration_minutes")]
    public int? DurationMinutes { get; set; }

    [Column("passing_score_percent")]
    public int PassingScorePercent { get; set; } = 60;

    [Column("max_attempts")]
    public int? MaxAttempts { get; set; }

    [Column("shuffle_questions")]
    public bool ShuffleQuestions { get; set; } = false;

    [Column("shuffle_options")]
    public bool ShuffleOptions { get; set; } = false;

    [Column("show_results_immediately")]
    public bool ShowResultsImmediately { get; set; } = true;

    [Column("allow_review")]
    public bool AllowReview { get; set; } = true;

    [Column("available_from")]
    public DateTime? AvailableFrom { get; set; }

    [Column("available_until")]
    public DateTime? AvailableUntil { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
}
