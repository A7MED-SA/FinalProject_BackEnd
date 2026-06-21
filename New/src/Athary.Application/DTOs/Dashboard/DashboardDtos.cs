namespace Athary.Application.DTOs.Dashboard;

public sealed record DashboardMetricDto
{
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public double? Change { get; init; }
    public string Trend { get; init; } = "neutral";
    public string? Icon { get; init; }
    public string? Color { get; init; }
}

public sealed record ChartSeriesDto
{
    public List<string> Labels { get; init; } = new();
    public List<SeriesItemDto> Series { get; init; } = new();
}

public sealed record SeriesItemDto
{
    public string Name { get; init; } = string.Empty;
    public List<double> Data { get; init; } = new();
}

public sealed record DistributionItemDto
{
    public string Label { get; init; } = string.Empty;
    public double Value { get; init; }
    public string? Color { get; set; }
    public double Percentage { get; set; }
}

public sealed record TopCourseDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? InstructorName { get; init; }
    public decimal Price { get; init; }
    public int EnrollmentCount { get; init; }
    public decimal AverageRating { get; init; }
    public decimal Revenue { get; init; }
}

public sealed record PendingItemsDto
{
    public int PendingCourses { get; init; }
    public int PendingEditRequests { get; init; }
    public int PendingTeacherRequests { get; init; }
    public int FlaggedReviews { get; init; }
}

// ─── Admin Dashboard ───────────────────────────────────────────────

public sealed record AdminOverviewDto
{
    public int TotalUsers { get; init; }
    public int TotalInstructors { get; init; }
    public int TotalCourses { get; init; }
    public decimal TotalRevenue { get; init; }
    public int TotalEnrollments { get; init; }
    public int PendingInstructors { get; init; }
    public int PendingCourses { get; init; }
}

public sealed record AdminRevenueDto
{
    public decimal TotalRevenue { get; init; }
    public ChartSeriesDto MonthlyRevenue { get; init; } = new();
    public List<DistributionItemDto> RevenueByCourse { get; init; } = new();
}

public sealed record AdminUserGrowthDto
{
    public int TotalUsers { get; init; }
    public ChartSeriesDto Growth { get; init; } = new();
    public List<DistributionItemDto> RoleDistribution { get; init; } = new();
}

public sealed record AdminEnrollmentTrendDto
{
    public ChartSeriesDto Trend { get; init; } = new();
    public List<DistributionItemDto> StatusDistribution { get; init; } = new();
}

// ─── Instructor Dashboard ──────────────────────────────────────────

public sealed record InstructorOverviewDto
{
    public List<DashboardMetricDto> Metrics { get; init; } = new();
    public List<InstructorCourseDto> Courses { get; init; } = new();
    public ChartSeriesDto RevenueTrend { get; init; } = new();
    public ChartSeriesDto EnrollmentTrend { get; init; } = new();
    public List<DistributionItemDto> StudentLevelDistribution { get; init; } = new();
    public int PendingEditRequests { get; init; }
}

public sealed record InstructorCourseDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public decimal Price { get; init; }
    public string Status { get; init; } = string.Empty;
    public int EnrollmentCount { get; init; }
    public decimal AverageRating { get; init; }
    public int TotalDurationMinutes { get; init; }
    public decimal Revenue { get; init; }
    public double ProgressPercentage { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? PublishedAt { get; init; }
}

// ─── Student Dashboard ─────────────────────────────────────────────

public sealed record StudentOverviewDto
{
    public List<DashboardMetricDto> Metrics { get; init; } = new();
    public List<StudentCourseDto> RecentCourses { get; init; } = new();
    public ChartSeriesDto WeeklyActivity { get; init; } = new();
    public List<StudentCertificateDto> RecentCertificates { get; init; } = new();
}

public sealed record StudentCourseDto
{
    public Guid Id { get; init; }
    public Guid EnrollmentId { get; init; }
    public Guid CourseId { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public string InstructorName { get; init; } = string.Empty;
    public double ProgressPercentage { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime LastAccessedAt { get; init; }
}

public sealed record StudentCertificateDto
{
    public Guid Id { get; init; }
    public Guid CourseId { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public string VerificationCode { get; init; } = string.Empty;
    public DateTime IssuedAt { get; init; }
    public string Status { get; init; } = string.Empty;
}

// ─── Course Management (Instructor) ─────────────────────────────────

public sealed record ManagementCourseDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ThumbnailUrl { get; init; }
    public decimal Price { get; init; }
    public string Status { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public int TotalDurationMinutes { get; init; }
    public int EnrollmentCount { get; init; }
    public decimal AverageRating { get; init; }
    public decimal Revenue { get; init; }
    public int SectionCount { get; init; }
    public int LessonCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed record ReviewSummaryDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string UserFullName { get; init; } = string.Empty;
    public string? UserProfileImageUrl { get; init; }
    public Guid CourseId { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public int Rating { get; init; }
    public string? Comment { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed record PendingEditRequestDto
{
    public Guid Id { get; init; }
    public Guid CourseId { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public string RequestType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime RequestedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public bool IsEmergency { get; init; }
    public string? ReviewerNote { get; init; }
}

public sealed record InstructorDashboardRevenueDto
{
    public decimal TotalRevenue { get; init; }
    public decimal CurrentMonthRevenue { get; init; }
    public decimal PreviousMonthRevenue { get; init; }
    public double RevenueChangePercent { get; init; }
    public ChartSeriesDto MonthlyBreakdown { get; init; } = new();
}

public sealed record InstructorDashboardStudentsDto
{
    public int TotalStudents { get; init; }
    public int ActiveStudents { get; init; }
    public int NewStudentsThisMonth { get; init; }
    public ChartSeriesDto EnrollmentOverTime { get; init; } = new();
}
