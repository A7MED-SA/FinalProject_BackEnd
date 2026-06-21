using Athary.Application.DTOs.Dashboard;

namespace Athary.Application.Interfaces.Dashboard;

public interface IStudentDashboardService
{
    Task<StudentOverviewDto> GetStudentOverviewAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<List<StudentCourseDto>> GetStudentCoursesAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<ChartSeriesDto> GetStudentWeeklyActivityAsync(Guid studentId, int weeks = 4, CancellationToken cancellationToken = default);
    Task<List<StudentCertificateDto>> GetStudentCertificatesAsync(Guid studentId, CancellationToken cancellationToken = default);
}
