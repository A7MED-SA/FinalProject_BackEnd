using Athary.Application.Common;
using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Athary.Application.Interfaces.Media;
using Athary.Domain.Enums;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Athary.Infrastructure.Services.Courses;

public sealed class PublicCourseService : IPublicCourseService
{
    private readonly ApplicationDbContext _context;
    private readonly IObjectStorage _objectStorage;
    private readonly ILogger<PublicCourseService> _logger;

    public PublicCourseService(
        ApplicationDbContext context,
        IObjectStorage objectStorage,
        ILogger<PublicCourseService> logger)
    {
        _context = context;
        _objectStorage = objectStorage;
        _logger = logger;
    }

    public async Task<PagedList<PublicCourseDto>> GetPublishedCoursesAsync(PublicCourseFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Courses
            .AsNoTracking()
            .Include(c => c.Creator)
            .Include(c => c.Category)
            .Include(c => c.Sections)
                .ThenInclude(s => s.SectionItems)
            .Where(c => c.IsPublished && c.Status == CourseStatus.Published && c.DeletedAt == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
        {
            var search = filter.SearchQuery.ToLower();
            query = query.Where(c =>
                c.Title.ToLower().Contains(search) ||
                (c.Description != null && c.Description.ToLower().Contains(search)));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(c => c.CategoryId == filter.CategoryId.Value);

        if (filter.Level.HasValue)
            query = query.Where(c => c.Level == filter.Level.Value);

        if (filter.Language.HasValue)
            query = query.Where(c => c.Language == filter.Language.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(c => c.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(c => c.Price <= filter.MaxPrice.Value);

        if (filter.IsFreeOnly == true)
            query = query.Where(c => c.Price == 0);

        if (filter.MinRating.HasValue)
            query = query.Where(c => c.AverageRating >= filter.MinRating.Value);

        query = filter.SortBy switch
        {
            PublicCourseSortBy.Price =>
                filter.SortDescending ? query.OrderByDescending(c => c.Price) : query.OrderBy(c => c.Price),
            PublicCourseSortBy.AverageRating =>
                filter.SortDescending ? query.OrderByDescending(c => c.AverageRating) : query.OrderBy(c => c.AverageRating),
            PublicCourseSortBy.EnrollmentCount =>
                filter.SortDescending ? query.OrderByDescending(c => c.EnrollmentCount) : query.OrderBy(c => c.EnrollmentCount),
            PublicCourseSortBy.Title =>
                filter.SortDescending ? query.OrderByDescending(c => c.Title) : query.OrderBy(c => c.Title),
            _ =>
                filter.SortDescending ? query.OrderByDescending(c => c.PublishedAt) : query.OrderBy(c => c.PublishedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(c => new PublicCourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Slug = c.Slug,
                Description = c.Description != null && c.Description.Length > 200
                    ? c.Description.Substring(0, 200) + "..."
                    : c.Description,
                Price = c.Price,
                Level = c.Level,
                Language = c.Language,
                CategoryName = c.Category.Name,
                CategoryId = c.CategoryId,
                InstructorName = c.Creator.FirstName + " " + c.Creator.LastName,
                AverageRating = c.AverageRating,
                EnrollmentCount = c.EnrollmentCount,
                TotalDurationMinutes = c.TotalDurationMinutes,
                SectionCount = c.Sections.Count,
                LessonCount = c.Sections.SelectMany(s => s.SectionItems).Count(),
                PublishedAt = c.PublishedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedList<PublicCourseDto>(items, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<PublicCourseDetailDto> GetCourseDetailsAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        var course = await GetPublishedCourseQuery()
            .FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken)
            ?? throw new KeyNotFoundException("Course not found or not published.");

        return MapToDetailDto(course);
    }

    public async Task<PublicCourseDetailDto> GetCourseDetailsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var course = await GetPublishedCourseQuery()
            .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken)
            ?? throw new KeyNotFoundException("Course not found or not published.");

        return MapToDetailDto(course);
    }

    public async Task<List<CourseSuggestionDto>> SearchCoursesSuggestAsync(string query, int limit = 5, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new List<CourseSuggestionDto>();

        var search = query.ToLower();

        return await _context.Courses
            .AsNoTracking()
            .Include(c => c.Category)
            .Where(c => c.IsPublished && c.Status == CourseStatus.Published && c.DeletedAt == null)
            .Where(c => c.Title.ToLower().Contains(search))
            .OrderByDescending(c => c.EnrollmentCount)
            .Take(limit)
            .Select(c => new CourseSuggestionDto
            {
                Id = c.Id,
                Title = c.Title,
                Slug = c.Slug,
                CategoryName = c.Category.Name
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PlatformStatsDto> GetPlatformStatsAsync(CancellationToken cancellationToken = default)
    {
        var totalCourses = await _context.Courses
            .AsNoTracking()
            .CountAsync(c => c.IsPublished && c.Status == CourseStatus.Published && c.DeletedAt == null, cancellationToken);

        var totalStudents = await _context.Enrollments
            .AsNoTracking()
            .Select(e => e.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var totalInstructors = await _context.Courses
            .AsNoTracking()
            .Where(c => c.IsPublished && c.DeletedAt == null)
            .Select(c => c.CreatedBy)
            .Distinct()
            .CountAsync(cancellationToken);

        var totalCategories = await _context.Categories
            .AsNoTracking()
            .CountAsync(c => c.IsActive, cancellationToken);

        return new PlatformStatsDto
        {
            TotalCourses = totalCourses,
            TotalStudents = totalStudents,
            TotalInstructors = totalInstructors,
            TotalCategories = totalCategories
        };
    }

    public async Task<List<PublicCourseDto>> GetRelatedCoursesAsync(Guid courseId, int limit = 4, CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken);

        if (course is null)
            return new List<PublicCourseDto>();

        return await _context.Courses
            .AsNoTracking()
            .Include(c => c.Creator)
            .Include(c => c.Category)
            .Include(c => c.Sections)
                .ThenInclude(s => s.SectionItems)
            .Where(c => c.IsPublished && c.Status == CourseStatus.Published && c.DeletedAt == null)
            .Where(c => c.Id != courseId)
            .Where(c => c.CategoryId == course.CategoryId || c.Level == course.Level)
            .OrderByDescending(c => c.AverageRating)
            .ThenByDescending(c => c.EnrollmentCount)
            .Take(limit)
            .Select(c => new PublicCourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Slug = c.Slug,
                Description = c.Description != null && c.Description.Length > 200
                    ? c.Description.Substring(0, 200) + "..."
                    : c.Description,
                Price = c.Price,
                Level = c.Level,
                Language = c.Language,
                CategoryName = c.Category.Name,
                CategoryId = c.CategoryId,
                InstructorName = c.Creator.FirstName + " " + c.Creator.LastName,
                AverageRating = c.AverageRating,
                EnrollmentCount = c.EnrollmentCount,
                TotalDurationMinutes = c.TotalDurationMinutes,
                SectionCount = c.Sections.Count,
                LessonCount = c.Sections.SelectMany(s => s.SectionItems).Count(),
                PublishedAt = c.PublishedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<FilterOptionsDto> GetFilterOptionsAsync(CancellationToken cancellationToken = default)
    {
        var publishedQuery = _context.Courses
            .AsNoTracking()
            .Where(c => c.IsPublished && c.Status == CourseStatus.Published && c.DeletedAt == null);

        var categories = await _context.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Select(c => new FilterOptionItem
            {
                Id = c.Id,
                Name = c.Name,
                CourseCount = publishedQuery.Count(co => co.CategoryId == c.Id)
            })
            .Where(c => c.CourseCount > 0)
            .OrderByDescending(c => c.CourseCount)
            .ToListAsync(cancellationToken);

        var prices = await publishedQuery
            .Select(c => c.Price)
            .ToListAsync(cancellationToken);

        return new FilterOptionsDto
        {
            Categories = categories,
            Levels = Enum.GetValues<CourseLevel>().ToList(),
            Languages = Enum.GetValues<CourseLanguage>().ToList(),
            MinPrice = prices.Count != 0 ? prices.Min() : 0,
            MaxPrice = prices.Count != 0 ? prices.Max() : 0
        };
    }

    private IQueryable<Domain.Entities.Course> GetPublishedCourseQuery()
    {
        return _context.Courses
            .AsNoTracking()
            .Include(c => c.Creator)
            .Include(c => c.Category)
            .Include(c => c.CourseRequirements)
            .Include(c => c.CourseLearningOutcomes)
            .Include(c => c.Sections)
                .ThenInclude(s => s.SectionItems)
            .Where(c => c.IsPublished && c.Status == CourseStatus.Published && c.DeletedAt == null);
    }

    private PublicCourseDetailDto MapToDetailDto(Domain.Entities.Course course)
    {
        return new PublicCourseDetailDto
        {
            Id = course.Id,
            Title = course.Title,
            Slug = course.Slug,
            Description = course.Description,
            Price = course.Price,
            Level = course.Level,
            Language = course.Language,
            CategoryName = course.Category?.Name ?? string.Empty,
            CategoryId = course.CategoryId,
            CourseImageUrl = course.CourseImageFile != null
                ? _objectStorage.GetPublicUrl(course.CourseImageFile.Bucket, course.CourseImageFile.FilePath)
                : null,
            IntroVideoUrl = course.IntroVideoFile != null
                ? _objectStorage.GetPublicUrl(course.IntroVideoFile.Bucket, course.IntroVideoFile.FilePath)
                : null,
            AverageRating = course.AverageRating,
            EnrollmentCount = course.EnrollmentCount,
            TotalDurationMinutes = course.TotalDurationMinutes,
            Version = course.Version,
            PublishedAt = course.PublishedAt,
            LastContentUpdateAt = course.LastContentUpdateAt,
            Instructor = new PublicInstructorDto
            {
                Id = course.Creator.Id,
                FullName = course.Creator.FullName,
                Bio = course.Creator.Bio,
            },
            Requirements = course.CourseRequirements
                .Select(r => r.Description)
                .ToList(),
            LearningOutcomes = course.CourseLearningOutcomes
                .Select(o => o.Description)
                .ToList(),
            Sections = course.Sections
                .OrderBy(s => s.Position)
                .Select(s => new PublicSectionDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    Position = s.Position,
                    Items = s.SectionItems
                        .OrderBy(si => si.Position)
                        .Select(si => new PublicSectionItemDto
                        {
                            Id = si.Id,
                            ItemType = si.ItemType,
                            Position = si.Position,
                            IsPreviewAllowed = si.IsPreviewAllowed
                        })
                        .ToList()
                })
                .ToList()
        };
    }
}
