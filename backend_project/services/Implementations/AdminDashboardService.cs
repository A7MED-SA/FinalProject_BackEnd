using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using backend_project.Data;
using backend_project.DTOs.Dashboard;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AdminDashboardService> _logger;

    public AdminDashboardService(
        ApplicationDbContext context,
        IMemoryCache cache,
        ILogger<AdminDashboardService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<AdminOverviewDto> GetOverviewAsync()
    {
        var errors = new List<DashboardError>();
        var dto = new AdminOverviewDto();

        try
        {
            dto.TotalUsers = await _context.Users.CountAsync(u => u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count users");
            errors.Add(new DashboardError { Section = "users", Message = "User count unavailable" });
        }

        try
        {
            dto.TotalCourses = await _context.Courses.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count courses");
            errors.Add(new DashboardError { Section = "courses", Message = "Course count unavailable" });
        }

        try
        {
            var grossRevenue = await _context.Orders
                .Where(o => o.Status == Models.OrderStatus.Completed)
                .SumAsync(o => (decimal?)o.FinalAmount) ?? 0;

            var refundedAmount = await _context.Refunds
                .Where(r => r.Status == RefundStatus.Approved)
                .SumAsync(r => (decimal?)r.Amount) ?? 0;

            dto.TotalRevenue = Math.Round(grossRevenue - refundedAmount, 2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute revenue");
            errors.Add(new DashboardError { Section = "revenue", Message = "Revenue data unavailable" });
        }

        try
        {
            dto.PendingCourseApprovals = await _context.Courses
                .CountAsync(c => c.Status == CourseStatus.PendingReview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count pending approvals");
            errors.Add(new DashboardError { Section = "pendingApprovals", Message = "Pending approvals unavailable" });
        }

        try
        {
            dto.PendingTeacherRequests = await _context.TeacherRequests
                .CountAsync(r => r.Status == backend_project.Models.TeacherRequestStatus.Pending);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count teacher requests");
            errors.Add(new DashboardError { Section = "teacherRequests", Message = "Teacher requests unavailable" });
        }

        try
        {
            dto.ActiveInstructors = await _context.Courses
                .Where(c => c.IsPublished)
                .Select(c => c.CreatedBy)
                .Distinct()
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count active instructors");
            errors.Add(new DashboardError { Section = "activeInstructors", Message = "Active instructors unavailable" });
        }

        dto.Errors = errors;
        return dto;
    }

    public async Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int months = 12)
    {
        var cacheKey = "AdminRevenueTrend";
        if (_cache.TryGetValue(cacheKey, out List<MonthlyRevenueDto>? cached))
            return cached!;

        try
        {
            var since = DateTime.UtcNow.AddMonths(-months);
            var revenueData = await _context.Orders
                .Where(o => o.Status == Models.OrderStatus.Completed && o.CreatedAt >= since)
                .GroupBy(o => new { Year = o.CreatedAt.Year, Month = o.CreatedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Gross = g.Sum(o => (decimal?)o.FinalAmount) ?? 0
                })
                .ToListAsync();

            var refundData = await _context.Refunds
                .Where(r => r.Status == RefundStatus.Approved && r.RequestedAt >= since)
                .GroupBy(r => new { Year = r.RequestedAt.Year, Month = r.RequestedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Refunded = g.Sum(r => (decimal?)r.Amount) ?? 0
                })
                .ToListAsync();

            var result = new List<MonthlyRevenueDto>();
            for (int i = months - 1; i >= 0; i--)
            {
                var dt = DateTime.UtcNow.AddMonths(-i);
                var monthLabel = dt.ToString("yyyy-MM");
                var gross = revenueData.FirstOrDefault(r => r.Year == dt.Year && r.Month == dt.Month)?.Gross ?? 0;
                var refunded = refundData.FirstOrDefault(r => r.Year == dt.Year && r.Month == dt.Month)?.Refunded ?? 0;
                result.Add(new MonthlyRevenueDto
                {
                    Month = monthLabel,
                    GrossAmount = Math.Round(gross, 2),
                    NetAmount = Math.Round(gross - refunded, 2)
                });
            }

            _cache.Set(cacheKey, result, DateTime.UtcNow.Date.AddDays(1));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute monthly revenue trends");
            return new List<MonthlyRevenueDto>();
        }
    }

    public async Task<List<UserGrowthDto>> GetUserGrowthAsync(int months = 6)
    {
        var cacheKey = "AdminUserGrowth";
        if (_cache.TryGetValue(cacheKey, out List<UserGrowthDto>? cached))
            return cached!;

        try
        {
            var since = DateTime.UtcNow.AddMonths(-months);
            var userData = await _context.Users
                .Where(u => u.CreatedAt >= since && u.IsActive)
                .GroupBy(u => new { Year = u.CreatedAt.Year, Month = u.CreatedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync();

            var result = new List<UserGrowthDto>();
            var cumulative = 0;
            for (int i = months - 1; i >= 0; i--)
            {
                var dt = DateTime.UtcNow.AddMonths(-i);
                var monthLabel = dt.ToString("yyyy-MM");
                var newCount = userData.FirstOrDefault(u => u.Year == dt.Year && u.Month == dt.Month)?.Count ?? 0;
                cumulative += newCount;
                result.Add(new UserGrowthDto
                {
                    Month = monthLabel,
                    NewUsers = newCount,
                    CumulativeTotal = cumulative
                });
            }

            _cache.Set(cacheKey, result, DateTime.UtcNow.Date.AddDays(1));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute user growth trends");
            return new List<UserGrowthDto>();
        }
    }

    public async Task<List<EnrollmentTrendDto>> GetEnrollmentTrendsAsync(int months = 12)
    {
        var cacheKey = "AdminEnrollmentTrends";
        if (_cache.TryGetValue(cacheKey, out List<EnrollmentTrendDto>? cached))
            return cached!;

        try
        {
            var since = DateTime.UtcNow.AddMonths(-months);
            var enrollmentData = await _context.Enrollments
                .Where(e => e.EnrolledAt >= since)
                .GroupBy(e => new { Year = e.EnrolledAt.Year, Month = e.EnrolledAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync();

            var result = new List<EnrollmentTrendDto>();
            for (int i = months - 1; i >= 0; i--)
            {
                var dt = DateTime.UtcNow.AddMonths(-i);
                var monthLabel = dt.ToString("yyyy-MM");
                var count = enrollmentData.FirstOrDefault(e => e.Year == dt.Year && e.Month == dt.Month)?.Count ?? 0;
                result.Add(new EnrollmentTrendDto
                {
                    Month = monthLabel,
                    Enrollments = count
                });
            }

            _cache.Set(cacheKey, result, DateTime.UtcNow.Date.AddDays(1));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute enrollment trends");
            return new List<EnrollmentTrendDto>();
        }
    }
}
