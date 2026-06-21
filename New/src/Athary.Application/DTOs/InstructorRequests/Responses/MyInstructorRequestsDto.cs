namespace Athary.Application.DTOs.InstructorRequests.Responses;

public class MyInstructorRequestsDto
{
    public List<InstructorRequestDto> Requests { get; set; } = new();
    public int TotalCount { get; set; }
    public bool CanSubmitNewRequest { get; set; }
}
