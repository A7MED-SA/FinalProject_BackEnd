using System;
using System.Collections.Generic;

namespace backend_project.DTOs.Quiz;

public class QuizResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? DurationMinutes { get; set; }
    public decimal PassingScorePercent { get; set; }
    public int? MaxAttempts { get; set; }
    public bool ShuffleQuestions { get; set; }
    public bool ShuffleOptions { get; set; }
    public bool ShowResultsImmediately { get; set; }
    public bool AllowReview { get; set; }
    public int TotalPoints { get; set; }
    public int QuestionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Will be populated when viewing a specific quiz details
    public IEnumerable<QuestionResponseDto> Questions { get; set; } = new List<QuestionResponseDto>();
}
