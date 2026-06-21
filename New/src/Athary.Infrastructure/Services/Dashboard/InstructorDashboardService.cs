using Athary.Application.DTOs.Dashboard;
using Athary.Application.Interfaces.Dashboard;
using Athary.Domain.Enums;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.Dashboard;

public sealed class InstructorDashboardService : IInstructorDashboardService
{
    private readonly ApplicationDbContext _context;

    public InstructorDashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InstructorOverviewDto> GetInstructorOverviewAsync(Guid instructorId, CancellationToken cancellationToken = default)
    {
        var courses = await _context.Courses
            .AsNoTracking()
            .Where(c => c.CreatedBy == instructorId && c.DeletedAt == null)
            .ToListAsync(cancellationToken);

        var courseIds = courses.Select(c => c.Id).ToList();
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalStudents = await _context.Enrollments
            .CountAsync(e => courseIds.Contains(e.CourseId), cancellationToken);

        var activeCourses = courses.Count(c => c.IsPublished);
        var totalDuration = courses.Sum(c => c.TotalDurationMinutes);
        var avgRating = courses.Count > 0 ? (double)Math.Round(courses.Average(c => c.AverageRating), 1) : 0;
        var totalRevenue = await _context.OrderItems
            .Where(oi => courseIds.Contains(oi.CourseId))
            .SumAsync(oi => (decimal?)oi.PriceAtPurchase ?? 0, cancellationToken);

        var monthRevenue = await _context.OrderItems
            .Where(oi => courseIds.Contains(oi.CourseId) && oi.AddedAt >= startOfMonth)
            .SumAsync(oi => (decimal?)oi.PriceAtPurchase ?? 0, cancellationToken);

        var prevMonthRevenue = await _context.OrderItems
            .Where(oi => courseIds.Contains(oi.CourseId) && oi.AddedAt >= startOfMonth.AddMonths(-1) && oi.AddedAt < startOfMonth)
            .SumAsync(oi => (decimal?)oi.PriceAtPurchase ?? 0, cancellationToken);

        var pendingEditRequests = await _context.CourseEditRequests
            .CountAsync(r => courseIds.Contains(r.CourseId) && r.Status == EditRequestStatus.Pending, cancellationToken);

        var revenueTrend = await GetInstructorRevenueTrendAsync(courseIds, 12, cancellationToken);
        var enrollmentTrend = await GetInstructorEnrollmentTrendAsync(courseIds, 12, cancellationToken);
        var studentLevelDist = await GetStudentLevelDistributionAsync(courseIds, cancellationToken);

        var courseDtos = courses.Select(c => new InstructorCourseDto
        {
            Id = c.Id,
            Title = c.Title,
            Slug = c.Slug,
            Price = c.Price,
            Status = c.Status.ToString(),
            EnrollmentCount = c.EnrollmentCount,
            AverageRating = c.AverageRating,
            TotalDurationMinutes = c.TotalDurationMinutes,
            Revenue = 0,
            ProgressPercentage = c.EnrollmentCount > 0 ? 75 : 0,
            CreatedAt = c.CreatedAt,
            PublishedAt = c.PublishedAt
        }).ToList();

        var revenueGrowth = CalculateChange(monthRevenue, prevMonthRevenue);
        var lastMonthStudents = await _context.Enrollments
            .CountAsync(e => courseIds.Contains(e.CourseId) && e.EnrolledAt >= startOfMonth.AddMonths(-1) && e.EnrolledAt < startOfMonth, cancellationToken);
        var newStudentsThisMonth = await _context.Enrollments
            .CountAsync(e => courseIds.Contains(e.CourseId) && e.EnrolledAt >= startOfMonth, cancellationToken);
        var studentGrowth = CalculateChange(newStudentsThisMonth, lastMonthStudents);

        return new InstructorOverviewDto
        {
            Metrics = new List<DashboardMetricDto>
            {
                new() { Label = "إجمالي الطلاب", Value = totalStudents.ToString("N0"), Icon = "Users", Color = "#4F46E5" },
                new() { Label = "الدورات النشطة", Value = activeCourses.ToString(), Icon = "BookOpen", Color = "#059669" },
                new() { Label = "ساعات التدريس", Value = $"{totalDuration / 60}h", Icon = "Clock", Color = "#D97706" },
                new() { Label = "متوسط التقييم", Value = avgRating.ToString("F1"), Icon = "Star", Color = "#F59E0B" },
                new() { Label = "الإيرادات", Value = totalRevenue.ToString("C2"), Change = revenueGrowth, Trend = revenueGrowth >= 0 ? "up" : "down", Icon = "DollarSign", Color = "#7C3AED" },
            },
            Courses = courseDtos,
            RevenueTrend = revenueTrend,
            EnrollmentTrend = enrollmentTrend,
            StudentLevelDistribution = studentLevelDist,
            PendingEditRequests = pendingEditRequests
        };
    }

    public async Task<List<ManagementCourseDto>> GetInstructorCoursesAsync(Guid instructorId, CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .AsNoTracking()
            .Include(c => c.Category)
            .Include(c => c.Sections)
                .ThenInclude(s => s.SectionItems)
            .Where(c => c.CreatedBy == instructorId && c.DeletedAt == null)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ManagementCourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Slug = c.Slug,
                Description = c.Description,
                Price = c.Price,
                Status = c.Status.ToString(),
                CategoryName = c.Category.Name,
                TotalDurationMinutes = c.TotalDurationMinutes,
                EnrollmentCount = c.EnrollmentCount,
                AverageRating = c.AverageRating,
                SectionCount = c.Sections.Count,
                LessonCount = c.Sections.SelectMany(s => s.SectionItems).Count(),
                CreatedAt = c.CreatedAt,
                PublishedAt = c.PublishedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<InstructorDashboardRevenueDto> GetInstructorRevenueAsync(Guid instructorId, int months = 12, CancellationToken cancellationToken = default)
    {
        var courseIds = await _context.Courses
            .Where(c => c.CreatedBy == instructorId && c.DeletedAt == null)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var totalRevenue = await _context.OrderItems
            .Where(oi => courseIds.Contains(oi.CourseId))
            .SumAsync(oi => (decimal?)oi.PriceAtPurchase ?? 0, cancellationToken);

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var currentMonthRevenue = await _context.OrderItems
            .Where(oi => courseIds.Contains(oi.CourseId) && oi.AddedAt >= startOfMonth)
            .SumAsync(oi => (decimal?)oi.PriceAtPurchase ?? 0, cancellationToken);

        var previousMonthRevenue = await _context.OrderItems
            .Where(oi => courseIds.Contains(oi.CourseId) && oi.AddedAt >= startOfMonth.AddMonths(-1) && oi.AddedAt < startOfMonth)
            .SumAsync(oi => (decimal?)oi.PriceAtPurchase ?? 0, cancellationToken);

        var monthlyBreakdown = await GetInstructorRevenueTrendAsync(courseIds, months, cancellationToken);

        return new InstructorDashboardRevenueDto
        {
            TotalRevenue = totalRevenue,
            CurrentMonthRevenue = currentMonthRevenue,
            PreviousMonthRevenue = previousMonthRevenue,
            RevenueChangePercent = CalculateChange(currentMonthRevenue, previousMonthRevenue),
            MonthlyBreakdown = monthlyBreakdown
        };
    }

    public async Task<InstructorDashboardStudentsDto> GetInstructorStudentsAsync(Guid instructorId, CancellationToken cancellationToken = default)
    {
        var courseIds = await _context.Courses
            .Where(c => c.CreatedBy == instructorId && c.DeletedAt == null)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var totalStudents = await _context.Enrollments
            .CountAsync(e => courseIds.Contains(e.CourseId), cancellationToken);

        var activeStudents = await _context.Enrollments
            .CountAsync(e => courseIds.Contains(e.CourseId) && e.Status == EnrollmentStatus.InProgress, cancellationToken);

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var newStudentsThisMonth = await _context.Enrollments
            .CountAsync(e => courseIds.Contains(e.CourseId) && e.EnrolledAt >= startOfMonth, cancellationToken);

        var enrollmentOverTime = await GetInstructorEnrollmentTrendAsync(courseIds, 12, cancellationToken);

        return new InstructorDashboardStudentsDto
        {
            TotalStudents = totalStudents,
            ActiveStudents = activeStudents,
            NewStudentsThisMonth = newStudentsThisMonth,
            EnrollmentOverTime = enrollmentOverTime
        };
    }

    public async Task<List<PendingEditRequestDto>> GetInstructorPendingRequestsAsync(Guid instructorId, CancellationToken cancellationToken = default)
    {
        return await _context.CourseEditRequests
            .AsNoTracking()
            .Include(r => r.Course)
            .Where(r => r.Course.CreatedBy == instructorId && r.Status == EditRequestStatus.Pending)
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => new PendingEditRequestDto
            {
                Id = r.Id,
                CourseId = r.CourseId,
                CourseTitle = r.Course.Title,
                RequestType = r.RequestType.ToString(),
                Status = r.Status.ToString(),
                RequestedAt = r.RequestedAt,
                ExpiresAt = r.ExpiresAt,
                IsEmergency = r.IsEmergency,
                ReviewerNote = r.AdminNotes
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ReviewSummaryDto>> GetInstructorRecentReviewsAsync(Guid instructorId, int limit = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Course)
            .Where(r => r.Course.CreatedBy == instructorId && r.Status == ReviewStatus.Approved && r.DeletedAt == null)
            .OrderByDescending(r => r.CreatedAt)
            .Take(limit)
            .Select(r => new ReviewSummaryDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserFullName = r.User.FirstName + " " + r.User.LastName,
                CourseId = r.CourseId,
                CourseTitle = r.Course.Title,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    // ─── Private Helpers ────────────────────────────────────────────

    private async Task<ChartSeriesDto> GetInstructorRevenueTrendAsync(List<Guid> courseIds, int months, CancellationToken cancellationToken)
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

            var revenue = await _context.OrderItems
                .Where(oi => courseIds.Contains(oi.CourseId) && oi.AddedAt >= monthStart && oi.AddedAt < monthEnd)
                .SumAsync(oi => (decimal?)oi.PriceAtPurchase ?? 0, cancellationToken);

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

    private async Task<ChartSeriesDto> GetInstructorEnrollmentTrendAsync(List<Guid> courseIds, int months, CancellationToken cancellationToken)
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
                .CountAsync(e => courseIds.Contains(e.CourseId) && e.EnrolledAt >= monthStart && e.EnrolledAt < monthEnd, cancellationToken);

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

    private async Task<List<DistributionItemDto>> GetStudentLevelDistributionAsync(List<Guid> courseIds, CancellationToken cancellationToken)
    {
        var distribution = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .Where(e => courseIds.Contains(e.CourseId))
            .GroupBy(e => e.Course.Level)
            .Select(g => new DistributionItemDto
            {
                Label = g.Key.ToString(),
                Value = g.Count()
            })
            .ToListAsync(cancellationToken);

        var total = distribution.Sum(d => d.Value);
        var colors = new[] { "#059669", "#D97706", "#4F46E5", "#DC2626" };
        for (var i = 0; i < distribution.Count; i++)
        {
            distribution[i].Percentage = total > 0 ? Math.Round(distribution[i].Value / total * 100, 1) : 0;
            distribution[i].Color = i < colors.Length ? colors[i] : "#6B7280";
        }

        return distribution;
    }

    private static double CalculateChange(decimal current, decimal previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return Math.Round((double)((current - previous) / previous * 100), 1);
    }
}
