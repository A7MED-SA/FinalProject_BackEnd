using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.Dashboard;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class InstructorDashboardService : IInstructorDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InstructorDashboardService> _logger;

    public InstructorDashboardService(ApplicationDbContext context, ILogger<InstructorDashboardService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<InstructorDashboardDto> GetDashboardAsync(Guid instructorId)
    {
        var errors = new List<DashboardError>();
        var dto = new InstructorDashboardDto();

        // Published courses count
        try
        {
            var courseQuery = _context.Courses.Where(c => c.CreatedBy == instructorId);

            dto.PublishedCourses = await courseQuery.CountAsync(c => c.IsPublished);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute course count for instructor {Id}", instructorId);
            errors.Add(new DashboardError { Section = "courses", Message = "Course data unavailable" });
        }

        // Total students (distinct enrollments across instructor's courses)
        try
        {
            dto.TotalStudents = await _context.Enrollments
                .Where(e => e.Course.CreatedBy == instructorId && e.Status != EnrollmentStatus.Refunded)
                .Select(e => e.UserId)
                .Distinct()
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute student count for instructor {Id}", instructorId);
            errors.Add(new DashboardError { Section = "students", Message = "Student count unavailable" });
        }

        // Total teaching hours (sum of course durations)
        try
        {
            dto.TotalTeachingHours = await _context.Courses
                .Where(c => c.CreatedBy == instructorId)
                .SumAsync(c => (int?)c.TotalDurationMinutes) ?? 0;
            dto.TotalTeachingHours = Math.Round(dto.TotalTeachingHours / 60m, 2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute teaching hours for instructor {Id}", instructorId);
            errors.Add(new DashboardError { Section = "teachingHours", Message = "Teaching hours unavailable" });
        }

        // Average rating across courses
        try
        {
            var ratings = await _context.Courses
                .Where(c => c.CreatedBy == instructorId)
                .Select(c => (decimal?)c.AverageRating)
                .ToListAsync();

            dto.AverageRating = ratings.Count > 0 ? Math.Round(ratings.Where(r => r.HasValue).Average() ?? 0, 2) : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute average rating for instructor {Id}", instructorId);
            errors.Add(new DashboardError { Section = "rating", Message = "Average rating unavailable" });
        }

        // Revenue
        try
        {
            var instructor = await _context.Users.FindAsync(instructorId);
            var sharePercent = instructor?.RevenueSharePercentage ?? 50.00m;

            var grossRevenue = await _context.OrderItems
                .Where(oi => oi.Course.CreatedBy == instructorId && oi.Order.Status == Models.OrderStatus.Completed)
                .SumAsync(oi => (decimal?)oi.PriceAtPurchase) ?? 0;

            var refundedCourseIds = await _context.Enrollments
                .Where(e => e.Course.CreatedBy == instructorId && e.IsRefunded)
                .Select(e => e.CourseId)
                .ToListAsync();

            var refundedAmount = refundedCourseIds.Count != 0
                ? await _context.OrderItems
                    .Where(oi => refundedCourseIds.Contains(oi.CourseId) && oi.Order.Status == Models.OrderStatus.Completed)
                    .SumAsync(oi => (decimal?)oi.PriceAtPurchase) ?? 0
                : 0;

            dto.GrossRevenue = Math.Round(grossRevenue - refundedAmount, 2);
            dto.TotalRevenue = Math.Round(dto.GrossRevenue * sharePercent / 100m, 2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute revenue for instructor {Id}", instructorId);
            errors.Add(new DashboardError { Section = "revenue", Message = "Revenue data unavailable" });
        }

        // Course list with enrollment counts and ratings
        try
        {
            dto.Courses = await _context.Courses
                .Where(c => c.CreatedBy == instructorId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new ManagementCourseDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Status = c.Status.ToString(),
                    IsPublished = c.IsPublished,
                    EnrollmentCount = c.EnrollmentCount,
                    AverageRating = c.AverageRating,
                    Price = c.Price,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load course list for instructor {Id}", instructorId);
            errors.Add(new DashboardError { Section = "courseList", Message = "Course list unavailable" });
        }

        // Pending edit requests
        try
        {
            dto.PendingEditRequests = await _context.CourseEditRequests
                .Where(r => r.Course.CreatedBy == instructorId && r.Status == EditRequestStatus.Pending)
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new PendingEditRequestDto
                {
                    Id = r.Id,
                    CourseTitle = r.Course.Title,
                    RequestType = r.RequestType.ToString(),
                    Operation = r.Operation.ToString(),
                    Status = r.Status.ToString(),
                    RequestedAt = r.RequestedAt,
                    IsEmergency = r.IsEmergency
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load pending edit requests for instructor {Id}", instructorId);
            errors.Add(new DashboardError { Section = "editRequests", Message = "Edit requests unavailable" });
        }

        dto.Errors = errors;
        return dto;
    }

    public async Task<PaginatedResult<ManagementCourseDto>> GetCoursesAsync(Guid instructorId, int page, int pageSize)
    {
        var query = _context.Courses
            .Where(c => c.CreatedBy == instructorId)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ManagementCourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Status = c.Status.ToString(),
                IsPublished = c.IsPublished,
                EnrollmentCount = c.EnrollmentCount,
                AverageRating = c.AverageRating,
                Price = c.Price,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResult<ManagementCourseDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
