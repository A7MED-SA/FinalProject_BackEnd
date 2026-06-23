using Athary.Application.DTOs.Category;
using Athary.Application.DTOs.Courses;
using Athary.Application.DTOs.LiveSession;
using Athary.Application.DTOs.Public;
using Athary.Application.Interfaces.Public;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Athary.Infrastructure.Services.Public;

public class PublicService : IPublicService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PublicService> _logger;

    public PublicService(
        ApplicationDbContext context,
        IMemoryCache cache,
        ILogger<PublicService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<LandingDto> GetLandingDataAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "landing_data_v1";

        if (_cache.TryGetValue(cacheKey, out LandingDto? cachedData) && cachedData is not null)
            return cachedData;

        try
        {
            var statsTask = GetLandingStatsAsync(cancellationToken);
            var categoriesTask = GetCategoriesAsync(cancellationToken);
            var featuredTask = GetFeaturedCoursesAsync(6, cancellationToken);
            var liveTask = GetUpcomingLiveSessionsAsync(4, cancellationToken);
            var testimonialsTask = GetApprovedTestimonialsAsync(10, cancellationToken);

            await Task.WhenAll(statsTask, categoriesTask, featuredTask, liveTask, testimonialsTask);

            var result = new LandingDto
            {
                Stats = await statsTask,
                Categories = await categoriesTask,
                FeaturedCourses = await featuredTask,
                UpcomingLiveSessions = await liveTask,
                Testimonials = await testimonialsTask
            };

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get landing data");
            return GetDefaultLandingData();
        }
    }

    private async Task<LandingStatsDto> GetLandingStatsAsync(CancellationToken cancellationToken)
    {
        var totalStudents = await _context.Users
            .CountAsync(u => u.DeletedAt == null, cancellationToken);

        var totalCourses = await _context.Courses
            .CountAsync(c => c.DeletedAt == null, cancellationToken);

        var totalInstructors = await _context.Users
            .CountAsync(u => u.DeletedAt == null, cancellationToken);

        var totalVideoHours = await _context.Videos
            .SumAsync(v => (double?)v.DurationSeconds, cancellationToken) ?? 0;

        var totalCertificates = await _context.Certificates
            .CountAsync(c => c.Status == Domain.Enums.CertificateStatus.Valid, cancellationToken);

        var satisfactionRate = await _context.Reviews
            .Where(r => r.DeletedAt == null)
            .AverageAsync(r => (double?)r.Rating, cancellationToken) ?? 0;

        return new LandingStatsDto
        {
            TotalStudents = totalStudents,
            TotalCourses = totalCourses,
            TotalInstructors = totalInstructors,
            SatisfactionRate = Math.Round(satisfactionRate / 5.0 * 100, 1),
            TotalVideoHours = (long)(totalVideoHours / 3600),
            TotalCertificatesIssued = totalCertificates
        };
    }

    private async Task<List<CategoryResponseDto>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        return await _context.Categories
            .Where(c => c.DeletedAt == null)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Slug = c.Slug,
                ImageUrl = c.CategoryImageFile != null ? c.CategoryImageFile.FilePath : null
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<PublicCourseDto>> GetFeaturedCoursesAsync(int count, CancellationToken cancellationToken)
    {
        var courseIds = await _context.Courses
            .Where(c => c.DeletedAt == null)
            .OrderByDescending(c => c.EnrollmentCount)
            .Take(count)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var courseIdsSet = courseIds.ToHashSet();

        var courses = await _context.Courses
            .Where(c => courseIdsSet.Contains(c.Id))
            .Select(c => new PublicCourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Slug = c.Slug,
                Description = c.Description,
                CourseImageUrl = c.CourseImageFile != null ? c.CourseImageFile.FilePath : null,
                Price = c.Price,
                Level = c.Level,
                Language = c.Language,
                CategoryName = c.Category.Name,
                CategoryId = c.CategoryId,
                InstructorName = $"{c.Creator.FirstName} {c.Creator.LastName}",
                EnrollmentCount = c.EnrollmentCount,
                TotalDurationMinutes = c.TotalDurationMinutes,
                SectionCount = c.Sections.Count,
                LessonCount = c.Sections.Sum(s => s.SectionItems.Count),
                PublishedAt = c.PublishedAt
            })
            .ToListAsync(cancellationToken);

        var ratings = await _context.Reviews
            .Where(r => courseIdsSet.Contains(r.CourseId) && r.DeletedAt == null)
            .GroupBy(r => r.CourseId)
            .Select(g => new { CourseId = g.Key, AvgRating = g.Average(r => r.Rating) })
            .ToListAsync(cancellationToken);

        var ratingMap = ratings.ToDictionary(r => r.CourseId, r => r.AvgRating);

        return courses.Select(c => c with
        {
            AverageRating = ratingMap.TryGetValue(c.Id, out var avg) ? (decimal)avg : 0
        }).ToList();
    }

    private async Task<List<LiveSessionResponseDto>> GetUpcomingLiveSessionsAsync(int count, CancellationToken cancellationToken)
    {
        return await _context.LiveSessions
            .Where(ls => ls.ScheduledStart > DateTime.UtcNow)
            .OrderBy(ls => ls.ScheduledStart)
            .Take(count)
            .Select(ls => new LiveSessionResponseDto
            {
                Id = ls.Id,
                Title = ls.Title,
                Description = ls.Description,
                ScheduledStart = ls.ScheduledStart,
                ScheduledEnd = ls.ScheduledEnd,
                Status = ls.Status,
                MeetingUrl = ls.MeetingUrl,
                MaxAttendees = ls.MaxAttendees,
                CurrentAttendeesCount = ls.LiveAttendances.Count
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<TestimonialDto>> GetApprovedTestimonialsAsync(int count, CancellationToken cancellationToken)
    {
        return await _context.Testimonials
            .Where(t => t.IsApproved && !t.IsFlagged)
            .OrderBy(t => t.DisplayOrder)
            .Take(count)
            .Select(t => new TestimonialDto
            {
                Id = t.Id,
                Content = t.Content,
                Rating = t.Rating,
                UserName = t.User.FullName,
                UserAvatar = t.User.ProfileImageFile != null ? t.User.ProfileImageFile.FilePath : null,
                DisplayOrder = t.DisplayOrder,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    private static LandingDto GetDefaultLandingData()
    {
        return new LandingDto
        {
            Stats = new LandingStatsDto(),
            Categories = new List<CategoryResponseDto>(),
            FeaturedCourses = new List<PublicCourseDto>(),
            UpcomingLiveSessions = new List<LiveSessionResponseDto>(),
            Testimonials = new List<TestimonialDto>()
        };
    }
}
