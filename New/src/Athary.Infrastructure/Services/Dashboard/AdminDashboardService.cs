using Athary.Application.DTOs.Dashboard;
using Athary.Application.Interfaces.Dashboard;
using Athary.Domain.Enums;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace Athary.Infrastructure.Services.Dashboard;

public sealed class AdminDashboardService : IAdminDashboardService
{
    private readonly ApplicationDbContext _context;

    public AdminDashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminOverviewDto> GetAdminOverviewAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await _context.Users.CountAsync(u => u.DeletedAt == null, cancellationToken);
        var totalCourses = await _context.Courses.CountAsync(c => c.DeletedAt == null, cancellationToken);
        var totalEnrollments = await _context.Enrollments.CountAsync(cancellationToken);
        var totalRevenue = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Succeeded)
            .SumAsync(p => (decimal?)p.Amount ?? 0, cancellationToken);
        var pendingCourses = await _context.Courses
            .CountAsync(c => c.Status == CourseStatus.PendingReview && c.DeletedAt == null, cancellationToken);
        var pendingInstructors = await _context.InstructorRequests
            .CountAsync(r => r.Status == InstructorRequestStatus.Pending, cancellationToken);

        var instructorRole = await _context.Roles
            .Where(r => r.Name == "Instructor")
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var totalInstructors = 0;
        if (instructorRole != Guid.Empty)
        {
            totalInstructors = await _context.UserRoles
                .CountAsync(ur => ur.RoleId == instructorRole, cancellationToken);
        }

        return new AdminOverviewDto
        {
            TotalUsers = totalUsers,
            TotalInstructors = totalInstructors,
            TotalCourses = totalCourses,
            TotalRevenue = totalRevenue,
            TotalEnrollments = totalEnrollments,
            PendingInstructors = pendingInstructors,
            PendingCourses = pendingCourses
        };
    }

    public async Task<AdminRevenueDto> GetAdminRevenueAsync(int months = 12, CancellationToken cancellationToken = default)
    {
        var totalRevenue = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Succeeded)
            .SumAsync(p => (decimal?)p.Amount ?? 0, cancellationToken);

        var revenueByCourse = await _context.OrderItems
            .AsNoTracking()
            .Include(oi => oi.Course)
            .GroupBy(oi => new { oi.CourseId, oi.Course.Title })
            .Select(g => new DistributionItemDto
            {
                Label = g.Key.Title,
                Value = (double)g.Sum(oi => oi.PriceAtPurchase)
            })
            .OrderByDescending(d => d.Value)
            .Take(10)
            .ToListAsync(cancellationToken);

        var total = revenueByCourse.Sum(d => d.Value);
        foreach (var item in revenueByCourse)
        {
            item.Percentage = total > 0 ? Math.Round(item.Value / total * 100, 1) : 0;
        }

        var monthlyRevenue = await GetRevenueTrendAsync(months, cancellationToken);

        return new AdminRevenueDto
        {
            TotalRevenue = totalRevenue,
            MonthlyRevenue = monthlyRevenue,
            RevenueByCourse = revenueByCourse
        };
    }

    public async Task<AdminUserGrowthDto> GetAdminUserGrowthAsync(int months = 6, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-(months - 1));

        var labels = new List<string>();
        var newUserData = new List<double>();
        var cumulativeData = new List<double>();
        var runningTotal = 0;

        for (var i = 0; i < months; i++)
        {
            var monthStart = startDate.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1);
            var count = await _context.Users
                .CountAsync(u => u.CreatedAt >= monthStart && u.CreatedAt < monthEnd, cancellationToken);

            runningTotal += count;
            labels.Add(monthStart.ToString("MMM yyyy"));
            newUserData.Add(count);
            cumulativeData.Add(runningTotal);
        }

        var totalUsers = runningTotal;

        var roleDistribution = await _context.UserRoles
            .AsNoTracking()
            .Include(ur => ur.Role)
            .GroupBy(ur => ur.Role.Name)
            .Select(g => new DistributionItemDto
            {
                Label = g.Key ?? "Unknown",
                Value = g.Count()
            })
            .ToListAsync(cancellationToken);

        var totalRoles = roleDistribution.Sum(d => d.Value);
        var roleColors = new[] { "#4F46E5", "#059669", "#D97706", "#DC2626", "#7C3AED", "#0891B2", "#DB2777" };
        for (var i = 0; i < roleDistribution.Count; i++)
        {
            roleDistribution[i].Percentage = totalRoles > 0 ? Math.Round(roleDistribution[i].Value / totalRoles * 100, 1) : 0;
            roleDistribution[i].Color = i < roleColors.Length ? roleColors[i] : "#6B7280";
        }

        return new AdminUserGrowthDto
        {
            TotalUsers = totalUsers,
            Growth = new ChartSeriesDto
            {
                Labels = labels,
                Series = new List<SeriesItemDto>
                {
                    new() { Name = "مستخدمين جدد", Data = newUserData },
                    new() { Name = "إجمالي المستخدمين", Data = cumulativeData }
                }
            },
            RoleDistribution = roleDistribution
        };
    }

    public async Task<AdminEnrollmentTrendDto> GetAdminEnrollmentTrendAsync(int months = 12, CancellationToken cancellationToken = default)
    {
        var trend = await GetEnrollmentTrendAsync(months, cancellationToken);

        var statusDistribution = await _context.Enrollments
            .AsNoTracking()
            .GroupBy(e => e.Status)
            .Select(g => new DistributionItemDto
            {
                Label = g.Key.ToString(),
                Value = g.Count()
            })
            .ToListAsync(cancellationToken);

        var total = statusDistribution.Sum(d => d.Value);
        var statusColors = new[] { "#059669", "#D97706", "#DC2626", "#6B7280", "#4F46E5" };
        for (var i = 0; i < statusDistribution.Count; i++)
        {
            statusDistribution[i].Percentage = total > 0 ? Math.Round(statusDistribution[i].Value / total * 100, 1) : 0;
            statusDistribution[i].Color = i < statusColors.Length ? statusColors[i] : "#6B7280";
        }

        return new AdminEnrollmentTrendDto
        {
            Trend = trend,
            StatusDistribution = statusDistribution
        };
    }

    public async Task<List<TopCourseDto>> GetAdminTopCoursesAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .AsNoTracking()
            .Include(c => c.Creator)
            .Where(c => c.DeletedAt == null && c.IsPublished)
            .OrderByDescending(c => c.EnrollmentCount)
            .Take(limit)
            .Select(c => new TopCourseDto
            {
                Id = c.Id,
                Title = c.Title,
                InstructorName = c.Creator.FirstName + " " + c.Creator.LastName,
                Price = c.Price,
                EnrollmentCount = c.EnrollmentCount,
                AverageRating = c.AverageRating,
                Revenue = 0
            })
            .ToListAsync(cancellationToken);
    }

    // ─── Private Helpers ────────────────────────────────────────────

    private async Task<ChartSeriesDto> GetRevenueTrendAsync(int months, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-(months - 1));

        var labels = new List<string>();
        var data = new List<double>();

        for (var i = 0; i < months; i++)
        {
            var monthStart = startDate.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1);
            labels.Add(monthStart.ToString("MMM yyyy"));

            var revenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Succeeded && p.PaidAt >= monthStart && p.PaidAt < monthEnd)
                .SumAsync(p => (decimal?)p.Amount ?? 0, cancellationToken);

            data.Add((double)revenue);
        }

        return new ChartSeriesDto
        {
            Labels = labels,
            Series = new List<SeriesItemDto>
            {
                new() { Name = "الإيرادات", Data = data }
            }
        };
    }

    private async Task<ChartSeriesDto> GetEnrollmentTrendAsync(int months, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-(months - 1));

        var labels = new List<string>();
        var data = new List<double>();

        for (var i = 0; i < months; i++)
        {
            var monthStart = startDate.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1);
            labels.Add(monthStart.ToString("MMM yyyy"));

            var count = await _context.Enrollments
                .CountAsync(e => e.EnrolledAt >= monthStart && e.EnrolledAt < monthEnd, cancellationToken);

            data.Add(count);
        }

        return new ChartSeriesDto
        {
            Labels = labels,
            Series = new List<SeriesItemDto>
            {
                new() { Name = "المسجلين", Data = data }
            }
        };
    }

    private async Task<ChartSeriesDto> GetUserGrowthAsync(int months, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-(months - 1));

        var labels = new List<string>();
        var data = new List<double>();

        for (var i = 0; i < months; i++)
        {
            var monthStart = startDate.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1);
            labels.Add(monthStart.ToString("MMM yyyy"));

            var count = await _context.Users
                .CountAsync(u => u.CreatedAt >= monthStart && u.CreatedAt < monthEnd, cancellationToken);

            data.Add(count);
        }

        return new ChartSeriesDto
        {
            Labels = labels,
            Series = new List<SeriesItemDto>
            {
                new() { Name = "مستخدمين جدد", Data = data }
            }
        };
    }

    private async Task<List<DistributionItemDto>> GetCourseDistributionAsync(CancellationToken cancellationToken)
    {
        var distribution = await _context.Courses
            .AsNoTracking()
            .Include(c => c.Category)
            .Where(c => c.DeletedAt == null && c.IsPublished)
            .GroupBy(c => c.Category.Name)
            .Select(g => new DistributionItemDto
            {
                Label = g.Key,
                Value = g.Count()
            })
            .OrderByDescending(d => d.Value)
            .ToListAsync(cancellationToken);

        var total = distribution.Sum(d => d.Value);
        var colorPalette = new[] { "#4F46E5", "#059669", "#D97706", "#DC2626", "#7C3AED", "#0891B2", "#DB2777", "#65A30D" };
        for (var i = 0; i < distribution.Count; i++)
        {
            distribution[i].Percentage = total > 0 ? Math.Round(distribution[i].Value / total * 100, 1) : 0;
            distribution[i].Color = i < colorPalette.Length ? colorPalette[i] : "#6B7280";
        }

        return distribution;
    }

    private static double CalculateChange(decimal current, decimal previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return Math.Round((double)((current - previous) / previous * 100), 1);
    }
}
