using Athary.Domain.Enums;

namespace Athary.Application.DTOs.InstructorRequests.Requests;

public class ProcessInstructorRequestDto
{
    public InstructorRequestStatus Status { get; set; }
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
}
