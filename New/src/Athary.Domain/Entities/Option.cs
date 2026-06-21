namespace Athary.Domain.Entities;

public sealed class Option : BaseEntity
{
    public Guid QuestionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int Position { get; set; }

    public Question Question { get; set; } = null!;
    public List<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
