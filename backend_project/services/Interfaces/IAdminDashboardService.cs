using backend_project.DTOs.Dashboard;

namespace backend_project.Services.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminOverviewDto> GetOverviewAsync();
    Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int months = 12);
    Task<List<UserGrowthDto>> GetUserGrowthAsync(int months = 6);
    Task<List<EnrollmentTrendDto>> GetEnrollmentTrendsAsync(int months = 12);
}
