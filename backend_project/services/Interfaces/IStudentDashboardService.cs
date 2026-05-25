using backend_project.DTOs.Dashboard;

namespace backend_project.Services.Interfaces;

public interface IStudentDashboardService
{
    Task<StudentDashboardDto> GetDashboardAsync(Guid userId);
}
