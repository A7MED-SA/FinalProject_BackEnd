using Athary.Domain.Enums;

namespace Athary.Application.DTOs.InstructorRequests.Responses;

public class InstructorRequestDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public InstructorRequestStatus Status { get; set; }
    public string? Message { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedByUserName { get; set; }
    public int DocumentsCount { get; set; }
}
