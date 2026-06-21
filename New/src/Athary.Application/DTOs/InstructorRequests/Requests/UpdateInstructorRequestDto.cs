namespace Athary.Application.DTOs.InstructorRequests.Requests;

public class UpdateInstructorRequestDto
{
    public string Message { get; set; } = string.Empty;
    public List<InstructorRequestDocumentDto> Documents { get; set; } = new();
}
