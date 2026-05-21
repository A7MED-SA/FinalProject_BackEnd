using System;
using System.Collections.Generic;

namespace backend_project.DTOs.QuizAttempt;

public class StartAttemptDto
{
    // Mostly empty since start attempt just requires the quiz ID (from route) and enrollment ID
    // But could include metadata if needed
}

public class SubmitAttemptDto
{
    public List<SubmitAnswerDto> Answers { get; set; } = new List<SubmitAnswerDto>();
}

public class SubmitAnswerDto
{
    public Guid QuestionId { get; set; }
    public Guid? SelectedOptionId { get; set; }
    public string? AnswerText { get; set; }
}

public class QuizAttemptResponseDto
{
    public Guid Id { get; set; }
    public Guid EnrollmentId { get; set; }
    public Guid QuizId { get; set; }
    public int AttemptNumber { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public decimal ScorePercentage { get; set; }
    public bool IsPassed { get; set; }
    public string Status { get; set; } = string.Empty; // InProgress, Submitted, Graded
}

public class QuizResultDto : QuizAttemptResponseDto
{
    public IEnumerable<AnswerResultDto> Answers { get; set; } = new List<AnswerResultDto>();
}

public class AnswerResultDto
{
    public Guid QuestionId { get; set; }
    public Guid? SelectedOptionId { get; set; }
    public string? AnswerText { get; set; }
    public bool IsCorrect { get; set; }
    public int EarnedPoints { get; set; }
}
