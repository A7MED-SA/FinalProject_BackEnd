using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.Models;
using backend_project.DTOs.Course;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly ApplicationDbContext _context;

    public CourseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourseDetailsDto> CreateCourseAsync(Guid instructorId, CreateCourseDto dto)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
            throw new KeyNotFoundException("Category not found.");

        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Slug = dto.Slug,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            Level = dto.Level,
            Language = dto.Language,
            Price = dto.Price,
            CreatedBy = instructorId,
            Status = CourseStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return await GetCourseByIdAsync(course.Id);
    }

    public async Task<CourseDetailsDto> UpdateCourseAsync(Guid courseId, Guid instructorId, UpdateCourseDto dto)
    {
        var course = await GetCourseForInstructorAsync(courseId, instructorId);

        course.Title = dto.Title;
        course.Slug = dto.Slug;
        course.Description = dto.Description;
        course.CategoryId = dto.CategoryId;
        course.Level = dto.Level;
        course.Language = dto.Language;
        course.Price = dto.Price;
        course.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetCourseByIdAsync(course.Id);
    }

    public async Task<CourseDetailsDto> GetCourseByIdAsync(Guid courseId)
    {
        var course = await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Creator)
            .Include(c => c.CourseRequirements)
            .Include(c => c.CourseLearningOutcomes)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            throw new KeyNotFoundException("Course not found.");

        return MapToDetailsDto(course);
    }

    public async Task<CourseRequirementDto> AddRequirementAsync(Guid courseId, Guid instructorId, AddRequirementDto dto)
    {
        var course = await GetCourseForInstructorAsync(courseId, instructorId);

        var req = new CourseRequirement
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            Description = dto.RequirementText
        };

        _context.CourseRequirements.Add(req);
        await _context.SaveChangesAsync();

        return new CourseRequirementDto { Id = req.Id, RequirementText = req.Description };
    }

    public async Task RemoveRequirementAsync(Guid courseId, Guid requirementId, Guid instructorId)
    {
        await GetCourseForInstructorAsync(courseId, instructorId);

        var req = await _context.CourseRequirements
            .FirstOrDefaultAsync(r => r.Id == requirementId && r.CourseId == courseId);

        if (req != null)
        {
            _context.CourseRequirements.Remove(req);
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
            Description = dto.OutcomeText
        };

        _context.CourseLearningOutcomes.Add(outcome);
        await _context.SaveChangesAsync();

        return new CourseLearningOutcomeDto { Id = outcome.Id, OutcomeText = outcome.Description };
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

        course.Status = CourseStatus.PendingReview;
        course.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<List<CourseSummaryDto>> GetPendingCoursesAsync()
    {
        var courses = await _context.Courses
            .Where(c => c.Status == CourseStatus.PendingReview)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return courses.Select(MapToSummaryDto).ToList();
    }

    public async Task ApproveCourseAsync(Guid courseId, Guid adminId)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
        if (course == null)
            throw new KeyNotFoundException("Course not found.");

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
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
        if (course == null)
            throw new KeyNotFoundException("Course not found.");

        if (course.Status != CourseStatus.PendingReview)
            throw new InvalidOperationException("Course is not pending review.");

        course.Status = CourseStatus.Draft;
        course.UpdatedAt = DateTime.UtcNow;
        // Optionally store the reason in a new CourseFeedback entity or ActivityLog
        
        await _context.SaveChangesAsync();
    }

    private async Task<Course> GetCourseForInstructorAsync(Guid courseId, Guid instructorId)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
        
        if (course == null)
            throw new KeyNotFoundException("Course not found.");

        if (course.CreatedBy != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to modify this course.");

        return course;
    }

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
            CreatorName = course.Creator != null ? $"{course.Creator.FirstName} {course.Creator.LastName}" : string.Empty,
            Level = course.Level,
            Language = course.Language,
            Price = course.Price,
            Status = course.Status,
            TotalDurationMinutes = course.TotalDurationMinutes,
            EnrollmentCount = course.EnrollmentCount,
            AverageRating = course.AverageRating,
            CreatedAt = course.CreatedAt,
            Requirements = course.CourseRequirements.Select(r => new CourseRequirementDto
            {
                Id = r.Id,
                RequirementText = r.Description
            }).ToList(),
            LearningOutcomes = course.CourseLearningOutcomes.Select(o => new CourseLearningOutcomeDto
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
            Description = course.Description,
            Price = course.Price,
            Level = course.Level,
            Language = course.Language,
            Status = course.Status,
            TotalDurationMinutes = course.TotalDurationMinutes,
            EnrollmentCount = course.EnrollmentCount,
            AverageRating = course.AverageRating,
            CreatedAt = course.CreatedAt
        };
    }
}
