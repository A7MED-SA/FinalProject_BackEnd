using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.Dashboard;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class StudentDashboardService : IStudentDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StudentDashboardService> _logger;

    public StudentDashboardService(ApplicationDbContext context, ILogger<StudentDashboardService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<StudentDashboardDto> GetDashboardAsync(Guid userId)
    {
        var errors = new List<DashboardError>();
        var dto = new StudentDashboardDto();

        // Stats aggregation
        try
        {
            var enrollments = _context.Enrollments
                .Where(e => e.UserId == userId && e.Status != EnrollmentStatus.Refunded);

            dto.TotalEnrolledCourses = await enrollments.CountAsync();
            dto.InProgressCourses = await enrollments.CountAsync(e => e.Status == EnrollmentStatus.InProgress);
            dto.CompletedCourses = await enrollments.CountAsync(e => e.Status == EnrollmentStatus.Completed);
            dto.CertificatesEarned = await enrollments.CountAsync(e => e.CertificateId != null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute student stats for user {UserId}", userId);
            errors.Add(new DashboardError { Section = "stats", Message = "Stats unavailable" });
        }

        // Learning hours
        try
        {
            var enrollmentIds = await _context.Enrollments
                .Where(e => e.UserId == userId && e.Status != EnrollmentStatus.Refunded)
                .Select(e => e.Id)
                .ToListAsync();

            if (enrollmentIds.Count != 0)
            {
                var videoSeconds = await _context.ContentProgresses
                    .Where(cp => enrollmentIds.Contains(cp.EnrollmentId) && cp.ContentType == ContentType.Video)
                    .SumAsync(cp => (int?)cp.WatchTimeSeconds) ?? 0;

                var quizCount = await _context.ContentProgresses
                    .Where(cp => enrollmentIds.Contains(cp.EnrollmentId) && cp.ContentType == ContentType.Quiz)
                    .CountAsync();

                var docCount = await _context.ContentProgresses
                    .Where(cp => enrollmentIds.Contains(cp.EnrollmentId) && cp.ContentType == ContentType.Document)
                    .CountAsync();

                decimal videoHours = videoSeconds / 3600m;
                decimal quizHours = quizCount * 0.5m;
                decimal docHours = docCount * 0.25m;

                dto.TotalLearningHours = videoHours + quizHours + docHours;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute learning hours for user {UserId}", userId);
            errors.Add(new DashboardError { Section = "learningHours", Message = "Learning hours unavailable" });
        }

        // Recent enrollments
        try
        {
            dto.RecentEnrollments = await _context.Enrollments
                .Where(e => e.UserId == userId && e.Status != EnrollmentStatus.Refunded)
                .OrderByDescending(e => e.LastAccessedAt ?? e.EnrolledAt)
                .Take(5)
                .Select(e => new EnrollmentBriefDto
                {
                    EnrollmentId = e.Id,
                    CourseId = e.CourseId,
                    CourseTitle = e.Course.Title,
                    ProgressPercentage = e.ProgressPercentage,
                    Status = e.Status.ToString(),
                    LastAccessedAt = e.LastAccessedAt,
                    CertificateId = e.CertificateId
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load recent enrollments for user {UserId}", userId);
            errors.Add(new DashboardError { Section = "recentEnrollments", Message = "Recent enrollments unavailable" });
        }

        // Certificate-eligible courses
        try
        {
            dto.CertificateEligibleCourses = await _context.Enrollments
                .Where(e => e.UserId == userId && e.Status == EnrollmentStatus.Completed && e.CertificateId == null)
                .Select(e => new EnrollmentBriefDto
                {
                    EnrollmentId = e.Id,
                    CourseId = e.CourseId,
                    CourseTitle = e.Course.Title,
                    ProgressPercentage = e.ProgressPercentage,
                    Status = e.Status.ToString(),
                    LastAccessedAt = e.LastAccessedAt,
                    CertificateId = e.CertificateId
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load certificate-eligible courses for user {UserId}", userId);
            errors.Add(new DashboardError { Section = "certificateEligible", Message = "Certificate-eligible courses unavailable" });
        }

        dto.Errors = errors;
        return dto;
    }
}
