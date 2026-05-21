using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using backend_project.Data;
using backend_project.Models;
using backend_project.DTOs.Course;
using backend_project.Services.Interfaces;
using backend_project.Configuration;

namespace backend_project.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly ApplicationDbContext _context;
    private readonly IObjectStorage _objectStorage;
    private readonly MinioSettings _minioSettings;
    private readonly ILogger<CourseService> _logger;

    public CourseService(
        ApplicationDbContext context,
        IObjectStorage objectStorage,
        IOptions<MinioSettings> minioSettings,
        ILogger<CourseService> logger)
    {
        _context = context;
        _objectStorage = objectStorage;
        _minioSettings = minioSettings.Value;
        _logger = logger;
    }

    #region 👨‍🏫 Instructor Operations

    public async Task<CourseDetailsDto> CreateCourseAsync(Guid instructorId, CreateCourseDto dto)
    {
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == dto.CategoryId && c.DeletedAt == null);
        
        if (!categoryExists)
            throw new KeyNotFoundException("Category not found.");

        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Slug = !string.IsNullOrWhiteSpace(dto.Slug) 
                ? dto.Slug 
                : GenerateSlug(dto.Title),
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            Level = dto.Level,
            Language = dto.Language,
            Price = dto.Price,
            CreatedBy = instructorId,
            Status = CourseStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return await GetCourseByIdAsync(course.Id);
    }

public async Task<CourseDetailsDto> UpdateCourseAsync(Guid courseId, Guid instructorId, UpdateCourseDto dto)
{
    var course = await GetCourseForInstructorAsync(courseId, instructorId);
    _logger.LogInformation("Course {courseId} {courseTitle} {dto.Title} {dto.Slug} is updating ...", courseId, course.Title, dto.Title, dto.Slug);
    // 1️⃣ تحديث الـ Title و الـ Slug
    if (!string.IsNullOrWhiteSpace(dto.Title))
    {
        course.Title = dto.Title;
        
        // لو الـ Slug ماتبعتش، نولّده من العنوان الجديد
        if (string.IsNullOrWhiteSpace(dto.Slug))
        {
            course.Slug = GenerateSlug(dto.Title);
        }
    }
    
    // لو المستخدم بعت Slug جديد صراحةً، نستخدمه
    if (!string.IsNullOrWhiteSpace(dto.Slug))
    {
        course.Slug = dto.Slug;
    }

    // 2️⃣ تحديث الحقول الأخرى (بس لو موجودة في الـ Request)
    if (dto.Description != null)
        course.Description = dto.Description;

    if (dto.CategoryId.HasValue)
    {
        // التحقق من وجود الفئة الجديدة
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == dto.CategoryId.Value && c.DeletedAt == null);
        if (!categoryExists)
            throw new KeyNotFoundException("Category not found.");
        
        course.CategoryId = dto.CategoryId.Value;
    }

    if (dto.Level.HasValue)
        course.Level = dto.Level.Value;

    if (dto.Language.HasValue)
        course.Language = dto.Language.Value;

    if (dto.Price.HasValue)
        course.Price = dto.Price.Value;

    course.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    return await GetCourseByIdAsync(course.Id);
}

    public async Task<CourseRequirementDto> AddRequirementAsync(Guid courseId, Guid instructorId, AddRequirementDto dto)
    {
        var course = await GetCourseForInstructorAsync(courseId, instructorId);

        var requirement = new CourseRequirement
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            Description = dto.RequirementText.Trim()
        };

        _context.CourseRequirements.Add(requirement);
        await _context.SaveChangesAsync();

        return new CourseRequirementDto 
        { 
            Id = requirement.Id, 
            RequirementText = requirement.Description 
        };
    }

    public async Task RemoveRequirementAsync(Guid courseId, Guid requirementId, Guid instructorId)
    {
        await GetCourseForInstructorAsync(courseId, instructorId);

        var requirement = await _context.CourseRequirements
            .FirstOrDefaultAsync(r => r.Id == requirementId && r.CourseId == courseId);

        if (requirement != null)
        {
            _context.CourseRequirements.Remove(requirement);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<CourseLearningOutcomeDto> AddLearningOutcomeAsync(Guid courseId, Guid instructorId, AddLearningOutcomeDto dto)
    {
        var course = await GetCourseForInstructorAsync(courseId, instructorId);

        var outcome = new CourseLearningOutcome
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            Description = dto.OutcomeText.Trim()
        };

        _context.CourseLearningOutcomes.Add(outcome);
        await _context.SaveChangesAsync();

        return new CourseLearningOutcomeDto 
        { 
            Id = outcome.Id, 
            OutcomeText = outcome.Description 
        };
    }

    public async Task RemoveLearningOutcomeAsync(Guid courseId, Guid outcomeId, Guid instructorId)
    {
        await GetCourseForInstructorAsync(courseId, instructorId);

        var outcome = await _context.CourseLearningOutcomes
            .FirstOrDefaultAsync(o => o.Id == outcomeId && o.CourseId == courseId);

        if (outcome != null)
        {
            _context.CourseLearningOutcomes.Remove(outcome);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SubmitForReviewAsync(Guid courseId, Guid instructorId)
    {
        var course = await GetCourseForInstructorAsync(courseId, instructorId);

        if (course.Status != CourseStatus.Draft)
            throw new InvalidOperationException("Only draft courses can be submitted for review.");

        if (string.IsNullOrWhiteSpace(course.Title) || 
            string.IsNullOrWhiteSpace(course.Description) || 
            course.CategoryId == Guid.Empty)
        {
            throw new InvalidOperationException("Course must have title, description, and category before submission.");
        }

        course.Status = CourseStatus.PendingReview;
        course.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    #endregion

    #region 👁️ Public Read Operations

    public async Task<CourseDetailsDto> GetCourseByIdAsync(Guid courseId)
    {
        var course = await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Creator)
            .Include(c => c.CourseRequirements)
            .Include(c => c.CourseLearningOutcomes)
            .Include(c => c.CourseImageFile)
            .FirstOrDefaultAsync(c => c.Id == courseId && c.DeletedAt == null);

        if (course == null)
            throw new KeyNotFoundException("Course not found.");

        return MapToDetailsDto(course);
    }

    public async Task<List<CourseSummaryDto>> GetPublishedCoursesAsync(CourseFilterDto? filters = null)
{
    // 1️⃣ ابدأ بالـ Query الأساسي بدون Includes
    var query = _context.Courses
        .Where(c => c.Status == CourseStatus.Published && c.DeletedAt == null)
        .AsQueryable();

    // 2️⃣ طبق الفلاتر على الـ Query الأساسي
    if (filters?.CategoryId.HasValue == true)
        query = query.Where(c => c.CategoryId == filters.CategoryId.Value);

    if (!string.IsNullOrWhiteSpace(filters?.SearchTerm))
    {
        var term = filters.SearchTerm.ToLower();
        query = query.Where(c => 
            c.Title.ToLower().Contains(term) || 
            c.Description.ToLower().Contains(term));
    }

    if (filters?.MinPrice.HasValue == true)
        query = query.Where(c => c.Price >= filters.MinPrice.Value);

    if (filters?.MaxPrice.HasValue == true)
        query = query.Where(c => c.Price <= filters.MaxPrice.Value);

    // 3️⃣ NOW أضف الـ Includes بعد ما تخلص من الفلاتر
    query = query
        .Include(c => c.Category)
        .Include(c => c.CourseImageFile);

    // 4️⃣ نفّذ الاستعلام وجيب البيانات
    var courses = await query.ToListAsync();

    // 5️⃣ رتّب في الذاكرة (بعد التنفيذ)
    courses = filters?.SortBy switch
    {
        "price_asc" => courses.OrderBy(c => c.Price).ToList(),
        "price_desc" => courses.OrderByDescending(c => c.Price).ToList(),
        "newest" => courses.OrderByDescending(c => c.CreatedAt).ToList(),
        _ => courses.OrderByDescending(c => c.CreatedAt).ToList()
    };

    return courses.Select(MapToSummaryDto).ToList();
}

    #endregion

    #region 👮 Admin Operations

    public async Task<List<CourseSummaryDto>> GetPendingCoursesAsync()
    {
        var courses = await _context.Courses
            .Where(c => c.Status == CourseStatus.PendingReview && c.DeletedAt == null)
            .Include(c => c.Creator)
            .Include(c => c.Category)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return courses.Select(MapToSummaryDto).ToList();
    }

    public async Task ApproveCourseAsync(Guid courseId, Guid adminId)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId && c.DeletedAt == null)
            ?? throw new KeyNotFoundException("Course not found.");

        if (course.Status != CourseStatus.PendingReview)
            throw new InvalidOperationException("Course is not pending review.");

        course.Status = CourseStatus.Published;
        course.IsPublished = true;
        course.ApprovedBy = adminId;
        course.PublishedAt = DateTime.UtcNow;
        course.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task RejectCourseAsync(Guid courseId, Guid adminId, string reason)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId && c.DeletedAt == null)
            ?? throw new KeyNotFoundException("Course not found.");

        if (course.Status != CourseStatus.PendingReview)
            throw new InvalidOperationException("Course is not pending review.");

        course.Status = CourseStatus.Draft;
        course.RejectionReason = reason;
        course.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteCourseAsync(Guid courseId, Guid instructorId)
    {
        var course = await GetCourseForInstructorAsync(courseId, instructorId);
        
        course.DeletedAt = DateTime.UtcNow;
        course.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCourseAsync(Guid courseId, Guid instructorId)
    {
        var course = await _context.Courses
            .Include(c => c.Sections)
                .ThenInclude(s => s.SectionItems)
            .FirstOrDefaultAsync(c => c.Id == courseId && c.DeletedAt == null)
            ?? throw new KeyNotFoundException("Course not found.");

        if (course.CreatedBy != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to delete this course.");

        bool hasEnrollments = await _context.Enrollments
            .AnyAsync(e => e.CourseId == courseId);

        if (course.Status == CourseStatus.Draft)
        {
            foreach (var section in course.Sections)
            {
                foreach (var item in section.SectionItems)
                {
                    await DeleteContentByItemAsync(item);
                }
                _context.SectionItems.RemoveRange(section.SectionItems);
            }
            _context.Sections.RemoveRange(course.Sections);
            _context.Courses.Remove(course);
        }
        else
        {
            course.DeletedAt = DateTime.UtcNow;
            course.UpdatedAt = DateTime.UtcNow;
            course.Status = hasEnrollments ? CourseStatus.Archived : CourseStatus.Archived;
        }

        await _context.SaveChangesAsync();
    }

    private async Task DeleteContentByItemAsync(SectionItem item)
    {
        switch (item.ItemType)
        {
            case SectionItemType.Video:
                var video = await _context.Videos.FindAsync(item.ItemId);
                if (video != null) _context.Videos.Remove(video);
                break;
            case SectionItemType.Document:
                var document = await _context.Documents.FindAsync(item.ItemId);
                if (document != null) _context.Documents.Remove(document);
                break;
            case SectionItemType.Quiz:
                var quiz = await _context.Quizzes
                    .Include(q => q.Questions).ThenInclude(qn => qn.Options)
                    .FirstOrDefaultAsync(q => q.Id == item.ItemId);
                if (quiz != null)
                {
                    foreach (var q in quiz.Questions)
                    {
                        _context.Options.RemoveRange(q.Options);
                    }
                    _context.Questions.RemoveRange(quiz.Questions);
                    _context.Quizzes.Remove(quiz);
                }
                break;
            case SectionItemType.LiveSession:
                var liveSession = await _context.LiveSessions.FindAsync(item.ItemId);
                if (liveSession != null) _context.LiveSessions.Remove(liveSession);
                break;
        }
    }

    #endregion

    #region 🖼️ Image Management

    public async Task<CourseDetailsDto> SetCourseImageAsync(Guid courseId, Guid fileId, Guid userId)
    {
        var course = await _context.Courses
            .Include(c => c.CourseImageFile)
            .FirstOrDefaultAsync(c => c.Id == courseId && c.DeletedAt == null)
            ?? throw new KeyNotFoundException("Course not found.");

        if (course.CreatedBy != userId)
            throw new UnauthorizedAccessException("You do not have permission to modify this course.");

        var file = await _context.Files.FirstOrDefaultAsync(f =>
            f.Id == fileId &&
            f.FileType == StoredFileType.Image &&
            f.Status == FileStatus.Ready &&
            f.DeletedAt == null)
            ?? throw new InvalidOperationException("Invalid image file");

        if (file.UploadedBy != userId)
            throw new UnauthorizedAccessException("You are not authorized to use this file");

        // Soft Delete للصورة القديمة
        if (course.CourseImageFileId.HasValue && course.CourseImageFileId != file.Id)
        {
            var oldFile = await _context.Files
                .FirstOrDefaultAsync(f => f.Id == course.CourseImageFileId && f.DeletedAt == null);

            if (oldFile != null)
            {
                oldFile.DeletedAt = DateTime.UtcNow;
                oldFile.Status = FileStatus.Deleted;
            }
        }

        file.Visibility = FileVisibility.Public;
        course.CourseImageFileId = file.Id;
        course.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetCourseByIdAsync(course.Id);
    }

    public async Task<CourseDetailsDto> RemoveCourseImageAsync(Guid courseId, Guid userId)
    {
        var course = await GetCourseForInstructorAsync(courseId, userId);

        if (!course.CourseImageFileId.HasValue)
            throw new InvalidOperationException("Course has no image to remove.");

        var file = await _context.Files
            .FirstOrDefaultAsync(f => f.Id == course.CourseImageFileId && f.DeletedAt == null);

        if (file != null)
        {
            file.DeletedAt = DateTime.UtcNow;
            file.Status = FileStatus.Deleted;
        }

        course.CourseImageFileId = null;
        course.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetCourseByIdAsync(course.Id);
    }

    #endregion

    #region 🔐 Helper Methods

    private async Task<Course> GetCourseForInstructorAsync(Guid courseId, Guid instructorId)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId && c.DeletedAt == null)
            ?? throw new KeyNotFoundException("Course not found.");

        if (course.CreatedBy != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to modify this course.");

        return course;
    }

    private string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return string.Empty;

        var slug = title.Trim().ToLowerInvariant();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-").Trim('-');
        
        while (slug.Contains("--"))
            slug = slug.Replace("--", "-");

        return slug;
    }

    #endregion

    #region 🗂️ DTO Mapping

    private CourseDetailsDto MapToDetailsDto(Course course)
    {
        return new CourseDetailsDto
        {
            Id = course.Id,
            Title = course.Title,
            Slug = course.Slug,
            Description = course.Description,
            CategoryId = course.CategoryId,
            CategoryName = course.Category?.Name ?? string.Empty,
            CreatedBy = course.CreatedBy,
            CreatorName = course.Creator != null 
                ? $"{course.Creator.FirstName} {course.Creator.LastName}".Trim() 
                : string.Empty,
            Level = course.Level,
            Language = course.Language,
            Price = course.Price,
            Status = course.Status,
            
            // الصورة
            ImageUrl = course.CourseImageFile != null
                ? _objectStorage.GetPublicUrl(
                    course.CourseImageFile.Bucket,
                    course.CourseImageFile.FilePath)
                : null,
            CourseImageFileId = course.CourseImageFileId,
            
            TotalDurationMinutes = course.TotalDurationMinutes,
            EnrollmentCount = course.EnrollmentCount,
            AverageRating = course.AverageRating,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt,
            PublishedAt = course.PublishedAt,
            
            Requirements = course.CourseRequirements
                .Select(r => new CourseRequirementDto
                {
                    Id = r.Id,
                    RequirementText = r.Description
                }).ToList(),
                
            LearningOutcomes = course.CourseLearningOutcomes
                .Select(o => new CourseLearningOutcomeDto
                {
                    Id = o.Id,
                    OutcomeText = o.Description
                }).ToList()
        };
    }

    private CourseSummaryDto MapToSummaryDto(Course course)
    {
        return new CourseSummaryDto
        {
            Id = course.Id,
            Title = course.Title,
            Slug = course.Slug,
            Description = course.Description?.Length > 200 
                ? course.Description.Substring(0, 200) + "..." 
                : course.Description,
            Price = course.Price,
            Level = course.Level,
            Language = course.Language,
            Status = course.Status,
            
            ThumbnailUrl = course.CourseImageFile != null
                ? _objectStorage.GetPublicUrl(
                    course.CourseImageFile.Bucket,
                    course.CourseImageFile.FilePath)
                : null,
                
            TotalDurationMinutes = course.TotalDurationMinutes,
            EnrollmentCount = course.EnrollmentCount,
            AverageRating = course.AverageRating,
            CategoryName = course.Category?.Name,
            CreatedAt = course.CreatedAt
        };
    }

    #endregion
}