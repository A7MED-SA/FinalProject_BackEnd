using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class Question : BaseEntity
{
    public Guid QuizId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType Type { get; set; } = QuestionType.MultipleChoice;
    public int Points { get; set; } = 1;
    public string? Explanation { get; set; }
    public int Position { get; set; }

    public Quiz Quiz { get; set; } = null!;
    public List<Option> Options { get; set; } = new List<Option>();
    public List<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
