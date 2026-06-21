using Athary.Domain.Enums;

namespace Athary.Application.DTOs.InstructorRequests.Requests;

public class AddDocumentToRequestDto
{
    public DocumentType DocumentType { get; set; }
    public Guid? FileId { get; set; }
    public string? UrlValue { get; set; }
}
