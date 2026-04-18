using backend_project.Models;
using backend_project.DTOs.TeacherRequests.Requests;

namespace backend_project.DTOs.TeacherRequests.Responses;

public class TeacherRequestDetailDto : TeacherRequestDto
{
    public List<TeacherRequestDocumentDto> Documents { get; set; } = new();
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
}