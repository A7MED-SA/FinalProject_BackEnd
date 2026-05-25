using backend_project.Models;

namespace backend_project.DTOs.Dashboard;

public class StudentDashboardDto
{
    public int TotalEnrolledCourses { get; set; }
    public int InProgressCourses { get; set; }
    public int CompletedCourses { get; set; }
    public decimal TotalLearningHours { get; set; }
    public int CertificatesEarned { get; set; }
    public List<EnrollmentBriefDto> RecentEnrollments { get; set; } = new();
    public List<EnrollmentBriefDto> CertificateEligibleCourses { get; set; } = new();
    public List<DashboardError> Errors { get; set; } = new();
}

public class EnrollmentBriefDto
{
    public Guid EnrollmentId { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public decimal ProgressPercentage { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? LastAccessedAt { get; set; }
    public Guid? CertificateId { get; set; }
}
