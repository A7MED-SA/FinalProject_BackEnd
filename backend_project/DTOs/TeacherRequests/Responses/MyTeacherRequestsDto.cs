using backend_project.Models;

namespace backend_project.DTOs.TeacherRequests.Responses;

public class MyTeacherRequestsDto
{
    public List<TeacherRequestDto> Requests { get; set; } = new();
    public int TotalCount { get; set; }
    public bool CanSubmitNewRequest { get; set; }
}