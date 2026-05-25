namespace backend_project.DTOs.Dashboard;

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class InstructorDashboardDto
{
    public int TotalStudents { get; set; }
    public int PublishedCourses { get; set; }
    public decimal TotalTeachingHours { get; set; }
    public decimal AverageRating { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal GrossRevenue { get; set; }
    public List<ManagementCourseDto> Courses { get; set; } = new();
    public List<PendingEditRequestDto> PendingEditRequests { get; set; } = new();
    public List<DashboardError> Errors { get; set; } = new();
}

public class ManagementCourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public int EnrollmentCount { get; set; }
    public decimal AverageRating { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PendingEditRequestDto
{
    public Guid Id { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public bool IsEmergency { get; set; }
}
