using backend_project.Models;

namespace backend_project.DTOs.TeacherRequests.Responses;

public class TeacherRequestDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public TeacherRequestStatus Status { get; set; }
    public string? Message { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedByUserName { get; set; }
    public int DocumentsCount { get; set; }
}