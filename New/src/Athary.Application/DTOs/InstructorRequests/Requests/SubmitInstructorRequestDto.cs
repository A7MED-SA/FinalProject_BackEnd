using Athary.Domain.Enums;

namespace Athary.Application.DTOs.InstructorRequests.Requests;

public class SubmitInstructorRequestDto
{
    public string Message { get; set; } = string.Empty;
    public List<InstructorRequestDocumentDto> Documents { get; set; } = new();
}

public class InstructorRequestDocumentDto
{
    public DocumentType DocumentType { get; set; }
    public Guid? FileId { get; set; }
    public string? UrlValue { get; set; }
}
