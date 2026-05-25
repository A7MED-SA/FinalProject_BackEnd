namespace backend_project.DTOs.Dashboard;

public class AdminOverviewDto
{
    public int TotalUsers { get; set; }
    public int TotalCourses { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingCourseApprovals { get; set; }
    public int PendingTeacherRequests { get; set; }
    public int ActiveInstructors { get; set; }
    public List<DashboardError> Errors { get; set; } = new();
}

public class MonthlyRevenueDto
{
    public string Month { get; set; } = string.Empty;
    public decimal GrossAmount { get; set; }
    public decimal NetAmount { get; set; }
}

public class UserGrowthDto
{
    public string Month { get; set; } = string.Empty;
    public int NewUsers { get; set; }
    public int CumulativeTotal { get; set; }
}

public class EnrollmentTrendDto
{
    public string Month { get; set; } = string.Empty;
    public int Enrollments { get; set; }
}

public class DashboardError
{
    public string Section { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
