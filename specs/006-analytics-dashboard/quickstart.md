# Quickstart: Analytics Dashboard

## Prerequisites

- Existing .NET 9.0 project with EF Core, Identity, JWT auth
- Existing models: Enrollment, ContentProgress, Course, Order, Payment, Refund, User, Review
- Existing services: IEnrollmentService, IActivityLogService

## New Files to Create

| File | Purpose |
|------|---------|
| `DTOs/Dashboard/StudentDashboardDtos.cs` | StudentDashboardDto, EnrollmentBriefDto |
| `DTOs/Dashboard/InstructorDashboardDtos.cs` | InstructorDashboardDto, ManagementCourseDto |
| `DTOs/Dashboard/AdminDashboardDtos.cs` | AdminOverviewDto, MonthlyRevenueDto, UserGrowthDto, DashboardError |
| `Services/Interfaces/IStudentDashboardService.cs` | Student dashboard interface |
| `Services/Interfaces/IInstructorDashboardService.cs` | Instructor dashboard interface |
| `Services/Interfaces/IAdminDashboardService.cs` | Admin dashboard interface |
| `Services/Implementations/StudentDashboardService.cs` | Student aggregation queries |
| `Services/Implementations/InstructorDashboardService.cs` | Instructor aggregation queries |
| `Services/Implementations/AdminDashboardService.cs` | Admin aggregation + trend data |
| `Controllers/DashboardController.cs` | Student + Instructor endpoints |
| `Controllers/AdminDashboardController.cs` | Admin overview + trends endpoints |
| `Controllers/ManagementCoursesController.cs` | Instructor course management |

## Files to Modify

| File | Change |
|------|--------|
| `Models/User.cs` | Add `RevenueSharePercentage` column (decimal, default 50.00) |
| `Data/ApplicationDbContext.cs` | Add entity configuration for RevenueSharePercentage |
| `Program.cs` | Register 3 dashboard services in DI |

## DI Registration (Program.cs)

```csharp
builder.Services.AddScoped<IStudentDashboardService, StudentDashboardService>();
builder.Services.AddScoped<IInstructorDashboardService, InstructorDashboardService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
```

## Aggregation Query Patterns

### Student Stats
```csharp
var stats = await _context.Enrollments
    .Where(e => e.UserId == userId && e.Status != EnrollmentStatus.Refunded)
    .GroupBy(e => 1)
    .Select(g => new {
        Total = g.Count(),
        InProgress = g.Count(e => e.Status == EnrollmentStatus.InProgress),
        Completed = g.Count(e => e.Status == EnrollmentStatus.Completed),
        Certificates = g.Count(e => e.CertificateId != null)
    })
    .FirstOrDefaultAsync();
```

### Revenue Per Instructor
```csharp
var revenue = await _context.Orders
    .Where(o => o.Status == OrderStatus.Completed)
    .Join(_context.OrderItems, o => o.Id, oi => oi.OrderId, (o, oi) => new { o, oi })
    .Join(_context.Courses, x => x.oi.CourseId, c => c.Id, (x, c) => new { x.o, c })
    .Where(x => x.c.CreatedBy == instructorId)
    .GroupBy(x => 1)
    .Select(g => new {
        Gross = g.Sum(x => (decimal?)x.o.TotalAmount) ?? 0
    })
    .FirstOrDefaultAsync();
```

## Cache Strategy

```csharp
// In AdminDashboardService constructor
private readonly IMemoryCache _cache;
private static readonly string RevenueCacheKey = "AdminRevenueTrend";
private static readonly string UserGrowthCacheKey = "AdminUserGrowth";

// Get from cache or compute
if (!_cache.TryGetValue(key, out List<MonthlyRevenueDto>? data))
{
    data = await ComputeMonthlyRevenueAsync();
    _cache.Set(key, data, TimeSpan.FromDays(1));
}
```

## Degraded Mode Pattern

```csharp
var errors = new List<DashboardError>();

DashboardStats stats;
try
{
    stats = await ComputeStudentStatsAsync(userId);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to compute student stats");
    errors.Add(new DashboardError { Section = "stats", Message = "Stats unavailable" });
    stats = new DashboardStats(); // zero defaults
}
```

## Controller Route Convention

```csharp
[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase { ... }

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase { ... }

[ApiController]
[Route("api/management")]
[Authorize(Roles = "Instructor")]
public class ManagementCoursesController : ControllerBase { ... }
```
