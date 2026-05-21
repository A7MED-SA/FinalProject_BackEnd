using System;

namespace backend_project.DTOs.Quiz;

public class CreateQuizDto
{
    public Guid SectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? DurationMinutes { get; set; }
    public decimal PassingScorePercent { get; set; } = 50;
    public int? MaxAttempts { get; set; }
    public bool ShuffleQuestions { get; set; } = false;
    public bool ShuffleOptions { get; set; } = false;
    public bool ShowResultsImmediately { get; set; } = true;
    public bool AllowReview { get; set; } = true;
}
