using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("questions")]
public class Question : BaseEntity
{

    [Required]
    [Column("quiz_id")]
    public Guid QuizId { get; set; }

    [Required]
    [Column("question_text")]
    public string QuestionText { get; set; } = string.Empty;

    [Column("type")]
    [MaxLength(20)]
    public QuestionType Type { get; set; } = QuestionType.MultipleChoice;

    [Column("points")]
    public int Points { get; set; } = 1;

    [Column("explanation")]
    public string? Explanation { get; set; }

    [Column("position")]
    public int Position { get; set; } = 0;

    // Navigation Properties
    [ForeignKey("QuizId")]
    public virtual Quiz Quiz { get; set; } = null!;

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();
    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}

public enum QuestionType
{
    MultipleChoice,
    TrueFalse,
    ShortAnswer
}
