using Athary.Application.DTOs.InstructorRequests.Requests;

namespace Athary.Application.DTOs.InstructorRequests.Responses;

public class InstructorRequestDetailDto : InstructorRequestDto
{
    public List<InstructorRequestDocumentDto> Documents { get; set; } = new();
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
}
