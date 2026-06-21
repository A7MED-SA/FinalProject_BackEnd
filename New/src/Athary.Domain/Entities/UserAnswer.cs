namespace Athary.Domain.Entities;

public sealed class UserAnswer : BaseEntity
{
    public Guid AttemptId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid? SelectedOptionId { get; set; }
    public string? AnswerText { get; set; }
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }

    public QuizAttempt Attempt { get; set; } = null!;
    public Question Question { get; set; } = null!;
    public Option? SelectedOption { get; set; }
}
