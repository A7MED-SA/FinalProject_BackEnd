using backend_project.DTOs.Dashboard;

namespace backend_project.Services.Interfaces;

public interface IInstructorDashboardService
{
    Task<InstructorDashboardDto> GetDashboardAsync(Guid instructorId);
    Task<PaginatedResult<ManagementCourseDto>> GetCoursesAsync(Guid instructorId, int page, int pageSize);
}
