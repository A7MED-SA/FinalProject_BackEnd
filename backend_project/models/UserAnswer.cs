using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("user_answers")]
public class UserAnswer : BaseEntity
{

    [Required]
    [Column("attempt_id")]
    public Guid AttemptId { get; set; }

    [Required]
    [Column("question_id")]
    public Guid QuestionId { get; set; }

    [Column("selected_option_id")]
    public Guid? SelectedOptionId { get; set; }

    [Column("answer_text")]
    public string? AnswerText { get; set; }

    [Column("is_correct")]
    public bool IsCorrect { get; set; } = false;

    [Column("points_earned")]
    public int PointsEarned { get; set; } = 0;

    // Navigation Properties
    [ForeignKey("AttemptId")]
    public virtual QuizAttempt Attempt { get; set; } = null!;

    [ForeignKey("QuestionId")]
    public virtual Question Question { get; set; } = null!;

    [ForeignKey("SelectedOptionId")]
    public virtual Option? SelectedOption { get; set; }
}
