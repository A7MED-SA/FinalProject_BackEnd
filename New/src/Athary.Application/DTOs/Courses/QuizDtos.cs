using Athary.Domain.Enums;

namespace Athary.Application.DTOs.Courses;

public sealed record CreateQuizDto
{
    public Guid SectionId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? DurationMinutes { get; init; }
    public int PassingScorePercent { get; init; } = 60;
    public int? MaxAttempts { get; init; }
    public bool ShuffleQuestions { get; init; }
    public bool ShuffleOptions { get; init; }
    public bool ShowResultsImmediately { get; init; } = true;
    public bool AllowReview { get; init; } = true;
    public DateTime? AvailableFrom { get; init; }
    public DateTime? AvailableUntil { get; init; }
}

public sealed record UpdateQuizDto
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? DurationMinutes { get; init; }
    public int PassingScorePercent { get; init; } = 60;
    public int? MaxAttempts { get; init; }
    public bool ShuffleQuestions { get; init; }
    public bool ShuffleOptions { get; init; }
    public bool ShowResultsImmediately { get; init; } = true;
    public bool AllowReview { get; init; } = true;
    public DateTime? AvailableFrom { get; init; }
    public DateTime? AvailableUntil { get; init; }
}

public sealed record QuizResponseDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? DurationMinutes { get; init; }
    public int PassingScorePercent { get; init; }
    public int? MaxAttempts { get; init; }
    public bool ShuffleQuestions { get; init; }
    public bool ShuffleOptions { get; init; }
    public bool ShowResultsImmediately { get; init; }
    public bool AllowReview { get; init; }
    public int TotalPoints { get; init; }
    public int QuestionCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<QuestionResponseDto> Questions { get; init; } = new();
}

public sealed record CreateQuestionDto
{
    public string QuestionText { get; init; } = string.Empty;
    public QuestionType Type { get; init; } = QuestionType.MultipleChoice;
    public int Points { get; init; } = 1;
    public string? Explanation { get; init; }
    public int Position { get; init; }
    public List<CreateOptionDto> Options { get; init; } = new();
}

public sealed record CreateOptionDto
{
    public string OptionText { get; init; } = string.Empty;
    public bool IsCorrect { get; init; }
    public int Position { get; init; }
}

public sealed record QuestionResponseDto
{
    public Guid Id { get; init; }
    public string QuestionText { get; init; } = string.Empty;
    public QuestionType Type { get; init; }
    public int Points { get; init; }
    public string? Explanation { get; init; }
    public int Position { get; init; }
    public List<OptionResponseDto> Options { get; init; } = new();
}

public sealed record OptionResponseDto
{
    public Guid Id { get; init; }
    public string OptionText { get; init; } = string.Empty;
    public bool? IsCorrect { get; init; }
    public int Position { get; init; }
}

public sealed record SubmitAnswerDto
{
    public Guid QuestionId { get; init; }
    public Guid? SelectedOptionId { get; init; }
    public string? AnswerText { get; init; }
}

public sealed record SubmitAttemptDto
{
    public List<SubmitAnswerDto> Answers { get; init; } = new();
}

public record QuizAttemptResponseDto
{
    public Guid Id { get; init; }
    public Guid EnrollmentId { get; init; }
    public Guid QuizId { get; init; }
    public int AttemptNumber { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public decimal ScorePercentage { get; init; }
    public bool IsPassed { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed record QuizResultDto : QuizAttemptResponseDto
{
    public bool IsAutoSubmitted { get; init; }
    public List<AnswerResultDto> Answers { get; init; } = new();
}

public sealed record AnswerResultDto
{
    public Guid QuestionId { get; init; }
    public Guid? SelectedOptionId { get; init; }
    public string? AnswerText { get; init; }
    public bool IsCorrect { get; init; }
    public int EarnedPoints { get; init; }
}
