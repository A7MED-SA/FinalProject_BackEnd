namespace Athary.Domain.Entities;

public sealed class Quiz : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DurationMinutes { get; set; }
    public int PassingScorePercent { get; set; } = 60;
    public int? MaxAttempts { get; set; }
    public bool ShuffleQuestions { get; set; }
    public bool ShuffleOptions { get; set; }
    public bool ShowResultsImmediately { get; set; } = true;
    public bool AllowReview { get; set; } = true;
    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableUntil { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public SectionItem? SectionItem { get; set; }
    public List<Question> Questions { get; set; } = new List<Question>();
    public List<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
}
