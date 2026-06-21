using Athary.Application.DTOs.Dashboard;
using Athary.Application.Interfaces.Dashboard;
using Athary.Domain.Enums;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.Dashboard;

public sealed class StudentDashboardService : IStudentDashboardService
{
    private readonly ApplicationDbContext _context;

    public StudentDashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentOverviewDto> GetStudentOverviewAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .Include(e => e.ContentProgresses)
            .Where(e => e.UserId == studentId)
            .ToListAsync(cancellationToken);

        var totalEnrolled = enrollments.Count;
        var inProgress = enrollments.Count(e => e.Status == EnrollmentStatus.InProgress);
        var completed = enrollments.Count(e => e.Status == EnrollmentStatus.Completed);
        var totalWatchSeconds = enrollments
            .SelectMany(e => e.ContentProgresses)
            .Sum(cp => cp.WatchTimeSeconds);
        var totalHours = totalWatchSeconds / 3600.0;
        var certificatesCount = enrollments.Count(e => e.CertificateId.HasValue);

        var weeklyActivity = await GetStudentWeeklyActivityAsync(studentId, 4, cancellationToken);

        var recentCourses = enrollments
            .OrderByDescending(e => e.LastAccessedAt ?? e.EnrolledAt)
            .Take(5)
            .Select(e => new StudentCourseDto
            {
                EnrollmentId = e.Id,
                CourseId = e.CourseId,
                CourseTitle = e.Course.Title,
                ProgressPercentage = (double)e.ProgressPercentage,
                Status = e.Status.ToString(),
                LastAccessedAt = e.LastAccessedAt ?? e.EnrolledAt
            })
            .ToList();

        var certificates = await GetStudentCertificatesAsync(studentId, cancellationToken);

        var overallProgress = totalEnrolled > 0
            ? Math.Round(enrollments.Average(e => (double)e.ProgressPercentage), 1)
            : 0;

        return new StudentOverviewDto
        {
            Metrics = new List<DashboardMetricDto>
            {
                new() { Label = "الدورات المسجلة", Value = totalEnrolled.ToString(), Icon = "BookOpen", Color = "#4F46E5" },
                new() { Label = "قيد التقدم", Value = inProgress.ToString(), Icon = "PlayCircle", Color = "#059669" },
                new() { Label = "الدورات المكتملة", Value = completed.ToString(), Icon = "CheckCircle", Color = "#D97706" },
                new() { Label = "ساعات التعلم", Value = totalHours.ToString("F1"), Icon = "Clock", Color = "#7C3AED" },
                new() { Label = "الشهادات", Value = certificatesCount.ToString(), Icon = "Award", Color = "#0891B2" },
            },
            RecentCourses = recentCourses,
            WeeklyActivity = weeklyActivity,
            RecentCertificates = certificates
        };
    }

    public async Task<List<StudentCourseDto>> GetStudentCoursesAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .Include(e => e.Course.Creator)
            .Where(e => e.UserId == studentId)
            .OrderByDescending(e => e.LastAccessedAt ?? e.EnrolledAt)
            .Select(e => new StudentCourseDto
            {
                EnrollmentId = e.Id,
                CourseId = e.CourseId,
                CourseTitle = e.Course.Title,
                InstructorName = e.Course.Creator.FirstName + " " + e.Course.Creator.LastName,
                ProgressPercentage = (double)e.ProgressPercentage,
                Status = e.Status.ToString(),
                LastAccessedAt = e.LastAccessedAt ?? e.EnrolledAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ChartSeriesDto> GetStudentWeeklyActivityAsync(Guid studentId, int weeks = 4, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var startDate = now.Date.AddDays(-(weeks * 7));

        var progressData = await _context.ContentProgresses
            .AsNoTracking()
            .Include(cp => cp.Enrollment)
            .Where(cp => cp.Enrollment.UserId == studentId && cp.LastAccessedAt >= startDate)
            .ToListAsync(cancellationToken);

        var labels = new List<string>();
        var data = new List<double>();

        for (var w = 0; w < weeks; w++)
        {
            var weekStart = now.Date.AddDays(-((weeks - w) * 7));
            var weekEnd = weekStart.AddDays(7);
            labels.Add($"الأسبوع {w + 1}");

            var weekSeconds = progressData
                .Where(cp => cp.LastAccessedAt >= weekStart && cp.LastAccessedAt < weekEnd)
                .Sum(cp => cp.WatchTimeSeconds);

            data.Add(Math.Round(weekSeconds / 60.0, 1));
        }

        return new ChartSeriesDto
        {
            Labels = labels,
            Series = new List<SeriesItemDto>
            {
                new() { Name = "دقائق التعلم", Data = data }
            }
        };
    }

    public async Task<List<StudentCertificateDto>> GetStudentCertificatesAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Certificates
            .AsNoTracking()
            .Include(c => c.Course)
            .Where(c => c.UserId == studentId && c.Status == CertificateStatus.Valid)
            .OrderByDescending(c => c.IssuedAt)
            .Select(c => new StudentCertificateDto
            {
                Id = c.Id,
                CourseId = c.CourseId,
                CourseTitle = c.Course.Title,
                VerificationCode = c.VerificationCode,
                IssuedAt = c.IssuedAt,
                Status = c.Status.ToString()
            })
            .ToListAsync(cancellationToken);
    }
}
