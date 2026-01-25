using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("options")]
public class Option : BaseEntity
{

    [Required]
    [Column("question_id")]
    public Guid QuestionId { get; set; }

    [Required]
    [Column("option_text")]
    [MaxLength(500)]
    public string OptionText { get; set; } = string.Empty;

    [Column("is_correct")]
    public bool IsCorrect { get; set; } = false;

    [Column("position")]
    public int Position { get; set; } = 0;

    // Navigation Properties
    [ForeignKey("QuestionId")]
    public virtual Question Question { get; set; } = null!;

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
