using Athary.Application.DTOs.Dashboard;

namespace Athary.Application.Interfaces.Dashboard;

public interface IAdminDashboardService
{
    Task<AdminOverviewDto> GetAdminOverviewAsync(CancellationToken cancellationToken = default);
    Task<AdminRevenueDto> GetAdminRevenueAsync(int months = 12, CancellationToken cancellationToken = default);
    Task<AdminUserGrowthDto> GetAdminUserGrowthAsync(int months = 6, CancellationToken cancellationToken = default);
    Task<AdminEnrollmentTrendDto> GetAdminEnrollmentTrendAsync(int months = 12, CancellationToken cancellationToken = default);
    Task<List<TopCourseDto>> GetAdminTopCoursesAsync(int limit = 10, CancellationToken cancellationToken = default);
}
