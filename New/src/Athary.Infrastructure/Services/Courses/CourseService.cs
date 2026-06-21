using System.Text.RegularExpressions;
using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Authentication;
using Athary.Application.Interfaces.Courses;
using Athary.Application.Interfaces.Media;
using Athary.Application.Interfaces.Notification;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Domain.Interfaces;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Athary.Infrastructure.Services.Courses;

public sealed class CourseService : ICourseService
{
    private readonly ApplicationDbContext _context;
    private readonly IRepository<Course> _courseRepo;
    private readonly IRepository<CourseRequirement> _requirementRepo;
    private readonly IRepository<CourseLearningOutcome> _outcomeRepo;
    private readonly IRepository<UploadedFile> _fileRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IObjectStorage _objectStorage;
    private readonly INotificationService _notificationService;
    private readonly IActivityLogService _activityLogService;
    private readonly ILogger<CourseService> _logger;

    public CourseService(
        ApplicationDbContext context,
        IRepository<Course> courseRepo,
        IRepository<CourseRequirement> requirementRepo,
        IRepository<CourseLearningOutcome> outcomeRepo,
        IRepository<UploadedFile> fileRepo,
        IUnitOfWork unitOfWork,
        IObjectStorage objectStorage,
        INotificationService notificationService,
        IActivityLogService activityLogService,
        ILogger<CourseService> logger)
    {
        _context = context;
        _courseRepo = courseRepo;
        _requirementRepo = requirementRepo;
        _outcomeRepo = outcomeRepo;
        _fileRepo = fileRepo;
        _unitOfWork = unitOfWork;
        _objectStorage = objectStorage;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
        _logger = logger;
    }

    public async Task<CourseDetailsDto> CreateCourseAsync(Guid instructorId, CreateCourseDto dto, CancellationToken cancellationToken = default)
    {
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == dto.CategoryId && c.DeletedAt == null, cancellationToken);

        if (!categoryExists)
            throw new KeyNotFoundException("Category not found.");

        var course = new Course
        {
            Title = dto.Title,
            Slug = !string.IsNullOrWhiteSpace(dto.Slug) ? dto.Slug : GenerateSlug(dto.Title),
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            Level = dto.Level,
            Language = dto.Language,
            Price = dto.Price,
            CreatedBy = instructorId,
            Status = CourseStatus.Draft
        };

        await _courseRepo.AddAsync(course, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Course created: {CourseId} by instructor {InstructorId}", course.Id, instructorId);

        return await GetCourseByIdAsync(course.Id, cancellationToken);
    }

    public async Task<CourseDetailsDto> UpdateCourseAsync(Guid courseId, Guid instructorId, UpdateCourseDto dto, CancellationToken cancellationToken = default)
    {
        var course = await GetCourseForInstructorAsync(courseId, instructorId, cancellationToken);

        if (dto.Title is not null)
        {
            course.Title = dto.Title;
            if (string.IsNullOrWhiteSpace(dto.Slug))
                course.Slug = GenerateSlug(dto.Title);
        }

        if (dto.Slug is not null)
            course.Slug = dto.Slug;

        if (dto.Description is not null)
            course.Description = dto.Description;

        if (dto.CategoryId.HasValue)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == dto.CategoryId.Value && c.DeletedAt == null, cancellationToken);
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Course updated: {CourseId}", courseId);

        return await GetCourseByIdAsync(course.Id, cancellationToken);
    }

    public async Task<CourseDetailsDto> GetCourseByIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Creator)
            .Include(c => c.CourseRequirements)
            .Include(c => c.CourseLearningOutcomes)
            .Include(c => c.CourseImageFile)
            .FirstOrDefaultAsync(c => c.Id == courseId && c.DeletedAt == null, cancellationToken)
            ?? throw new KeyNotFoundException("Course not found.");

        return MapToDetailsDto(course);
    }

    public async Task<CourseRequirementDto> AddRequirementAsync(Guid courseId, Guid instructorId, AddRequirementDto dto, CancellationToken cancellationToken = default)
    {
        await GetCourseForInstructorAsync(courseId, instructorId, cancellationToken);

        var requirement = new CourseRequirement
        {
            CourseId = courseId,
            Description = dto.RequirementText.Trim()
        };

        await _requirementRepo.AddAsync(requirement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CourseRequirementDto
        {
            Id = requirement.Id,
            RequirementText = requirement.Description
        };
    }

    public async Task RemoveRequirementAsync(Guid courseId, Guid requirementId, Guid instructorId, CancellationToken cancellationToken = default)
    {
        await GetCourseForInstructorAsync(courseId, instructorId, cancellationToken);

        var requirement = await _requirementRepo.FirstOrDefaultAsync(
            r => r.Id == requirementId && r.CourseId == courseId,
            cancellationToken: cancellationToken);

        if (requirement is not null)
        {
            await _requirementRepo.DeleteAsync(requirement, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<CourseLearningOutcomeDto> AddLearningOutcomeAsync(Guid courseId, Guid instructorId, AddLearningOutcomeDto dto, CancellationToken cancellationToken = default)
    {
        await GetCourseForInstructorAsync(courseId, instructorId, cancellationToken);

        var outcome = new CourseLearningOutcome
        {
            CourseId = courseId,
            Description = dto.OutcomeText.Trim()
        };

        await _outcomeRepo.AddAsync(outcome, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CourseLearningOutcomeDto
        {
            Id = outcome.Id,
            OutcomeText = outcome.Description
        };
    }

    public async Task RemoveLearningOutcomeAsync(Guid courseId, Guid outcomeId, Guid instructorId, CancellationToken cancellationToken = default)
    {
        await GetCourseForInstructorAsync(courseId, instructorId, cancellationToken);

        var outcome = await _outcomeRepo.FirstOrDefaultAsync(
            o => o.Id == outcomeId && o.CourseId == courseId,
            cancellationToken: cancellationToken);

        if (outcome is not null)
        {
            await _outcomeRepo.DeleteAsync(outcome, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task SubmitForReviewAsync(Guid courseId, Guid instructorId, CancellationToken cancellationToken = default)
    {
        var course = await GetCourseForInstructorAsync(courseId, instructorId, cancellationToken);

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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Course submitted for review: {CourseId}", courseId);
    }

    public async Task ApproveCourseAsync(Guid courseId, Guid adminId, CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId && c.DeletedAt == null, cancellationToken)
            ?? throw new KeyNotFoundException("Course not found.");

        if (course.Status != CourseStatus.PendingReview)
            throw new InvalidOperationException("Course is not pending review.");

        course.Status = CourseStatus.Published;
        course.IsPublished = true;
        course.ApprovedBy = adminId;
        course.PublishedAt = DateTime.UtcNow;
        course.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Course approved: {CourseId} by admin {AdminId}", courseId, adminId);
    }

    public async Task RejectCourseAsync(Guid courseId, Guid adminId, string reason, CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId && c.DeletedAt == null, cancellationToken)
            ?? throw new KeyNotFoundException("Course not found.");

        if (course.Status != CourseStatus.PendingReview)
            throw new InvalidOperationException("Course is not pending review.");

        course.Status = CourseStatus.Draft;
        course.RejectionReason = reason;
        course.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Course rejected: {CourseId} by admin {AdminId}", courseId, adminId);
    }

    public async Task<CourseDetailsDto> SetCourseImageAsync(Guid courseId, Guid fileId, Guid userId, CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses
            .Include(c => c.CourseImageFile)
            .FirstOrDefaultAsync(c => c.Id == courseId && c.DeletedAt == null, cancellationToken)
            ?? throw new KeyNotFoundException("Course not found.");

        if (course.CreatedBy != userId)
            throw new UnauthorizedAccessException("You do not have permission to modify this course.");

        var file = await _fileRepo.FirstOrDefaultAsync(f =>
            f.Id == fileId &&
            f.FileType == StoredFileType.Image &&
            f.Status == FileStatus.Ready &&
            f.DeletedAt == null, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Invalid image file");

        if (file.UploadedBy != userId)
            throw new UnauthorizedAccessException("You are not authorized to use this file");

        if (course.CourseImageFileId.HasValue && course.CourseImageFileId != file.Id)
        {
            var oldFile = await _fileRepo.FirstOrDefaultAsync(
                f => f.Id == course.CourseImageFileId && f.DeletedAt == null,
                cancellationToken: cancellationToken);

            if (oldFile is not null)
            {
                oldFile.DeletedAt = DateTime.UtcNow;
                oldFile.Status = FileStatus.Deleted;
            }
        }

        file.Visibility = FileVisibility.Public;
        course.CourseImageFileId = file.Id;
        course.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Course image set: {CourseId}, file {FileId}", courseId, fileId);

        course.CourseImageFile = file;
        return MapToDetailsDto(course);
    }

    public async Task<CourseDetailsDto> RemoveCourseImageAsync(Guid courseId, Guid userId, CancellationToken cancellationToken = default)
    {
        var course = await _courseRepo.FirstOrDefaultAsync(
            c => c.Id == courseId && c.DeletedAt == null,
            q => q.Include(c => c.CourseImageFile),
            cancellationToken)
            ?? throw new KeyNotFoundException("Course not found.");

        if (course.CreatedBy != userId)
            throw new UnauthorizedAccessException("You do not have permission to modify this course.");

        if (!course.CourseImageFileId.HasValue)
            throw new InvalidOperationException("Course has no image to remove.");

        var file = await _fileRepo.FirstOrDefaultAsync(
            f => f.Id == course.CourseImageFileId && f.DeletedAt == null,
            cancellationToken: cancellationToken);

        if (file is not null)
        {
            file.DeletedAt = DateTime.UtcNow;
            file.Status = FileStatus.Deleted;
        }

        course.CourseImageFileId = null;
        course.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Course image removed: {CourseId}", courseId);

        return await GetCourseByIdAsync(course.Id, cancellationToken);
    }

    public async Task DeleteCourseAsync(Guid courseId, Guid instructorId, CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses
            .Include(c => c.Sections)
                .ThenInclude(s => s.SectionItems)
            .FirstOrDefaultAsync(c => c.Id == courseId && c.DeletedAt == null, cancellationToken)
            ?? throw new KeyNotFoundException("Course not found.");

        if (course.CreatedBy != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to delete this course.");

        bool hasEnrollments = await _context.Enrollments
            .AnyAsync(e => e.CourseId == courseId, cancellationToken);

        if (course.Status == CourseStatus.Draft)
        {
            foreach (var section in course.Sections)
            {
                foreach (var item in section.SectionItems)
                    await DeleteContentByItemAsync(item, cancellationToken);

                _context.SectionItems.RemoveRange(section.SectionItems);
            }
            _context.Sections.RemoveRange(course.Sections);
            _context.Courses.Remove(course);
        }
        else if (hasEnrollments)
        {
            throw new InvalidOperationException(
                "Cannot delete a published course with enrolled students. " +
                "Use ScheduleDeletionAsync to schedule deletion instead.");
        }
        else
        {
            course.DeletedAt = DateTime.UtcNow;
            course.UpdatedAt = DateTime.UtcNow;
            course.Status = CourseStatus.Archived;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Course deleted: {CourseId}", courseId);
    }

    public async Task ScheduleDeletionAsync(Guid courseId, Guid instructorId, DateTime scheduledDate, string? reason, CancellationToken cancellationToken = default)
    {
        var course = await _courseRepo.FirstOrDefaultAsync(
            c => c.Id == courseId && c.DeletedAt == null,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException("Course not found.");

        if (course.CreatedBy != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to modify this course.");

        if (course.Status != CourseStatus.Published)
            throw new InvalidOperationException("Only published courses can be scheduled for deletion.");

        bool hasEnrollments = await _context.Enrollments
            .AnyAsync(e => e.CourseId == courseId, cancellationToken);

        if (!hasEnrollments)
        {
            course.DeletedAt = DateTime.UtcNow;
            course.UpdatedAt = DateTime.UtcNow;
            course.Status = CourseStatus.Archived;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        if (scheduledDate <= DateTime.UtcNow)
            throw new InvalidOperationException("Scheduled date must be in the future.");

        if (scheduledDate > DateTime.UtcNow.AddMonths(6))
            throw new InvalidOperationException("Scheduled date cannot be more than 6 months in the future.");

        course.ScheduledDeletionAt = scheduledDate;
        course.DeletionReason = reason;
        course.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var enrolledStudents = await _context.Enrollments
            .Where(e => e.CourseId == courseId &&
                       (e.Status == EnrollmentStatus.InProgress || e.Status == EnrollmentStatus.Completed))
            .Select(e => e.UserId)
            .ToListAsync(cancellationToken);

        foreach (var studentId in enrolledStudents)
        {
            await _notificationService.CreateAndSendNotificationAsync(
                studentId,
                "⚠️ Course Scheduled for Deletion",
                $"The course \"{course.Title}\" has been scheduled for deletion on {scheduledDate:yyyy-MM-dd}. " +
                $"Please complete your work before this date. After that, the course will become read-only." +
                (string.IsNullOrEmpty(reason) ? "" : $"\nReason: {reason}"),
                NotificationType.Course,
                linkUrl: $"/courses/{courseId}",
                cancellationToken: cancellationToken);
        }

        await _activityLogService.LogActivityAsync(
            userId: instructorId,
            action: "Course.DeletionScheduled",
            description: $"Scheduled deletion for course '{course.Title}' on {scheduledDate:yyyy-MM-dd}",
            ipAddress: "system",
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Deletion scheduled for course {CourseId} on {ScheduledDate} by instructor {InstructorId}",
            courseId, scheduledDate, instructorId);
    }

    public async Task CancelScheduledDeletionAsync(Guid courseId, Guid instructorId, CancellationToken cancellationToken = default)
    {
        var course = await _courseRepo.FirstOrDefaultAsync(
            c => c.Id == courseId && c.DeletedAt == null,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException("Course not found.");

        if (course.CreatedBy != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to modify this course.");

        if (course.ScheduledDeletionAt == null)
            throw new InvalidOperationException("Course has no scheduled deletion.");

        if (course.IsReadOnlyForStudents)
            throw new InvalidOperationException("Cannot cancel deletion after it has been executed.");

        course.ScheduledDeletionAt = null;
        course.DeletionReason = null;
        course.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var enrolledStudents = await _context.Enrollments
            .Where(e => e.CourseId == courseId &&
                       (e.Status == EnrollmentStatus.InProgress || e.Status == EnrollmentStatus.Completed))
            .Select(e => e.UserId)
            .ToListAsync(cancellationToken);

        foreach (var studentId in enrolledStudents)
        {
            await _notificationService.CreateAndSendNotificationAsync(
                studentId,
                "✅ Deletion Cancelled",
                $"The scheduled deletion for course \"{course.Title}\" has been cancelled.",
                NotificationType.Course,
                linkUrl: $"/courses/{courseId}",
                cancellationToken: cancellationToken);
        }

        await _activityLogService.LogActivityAsync(
            userId: instructorId,
            action: "Course.DeletionCancelled",
            description: $"Cancelled scheduled deletion for course '{course.Title}'",
            ipAddress: "system",
            cancellationToken: cancellationToken);
    }

    public async Task<ScheduledDeletionStatusDto> GetScheduledDeletionStatusAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == courseId && c.DeletedAt == null, cancellationToken)
            ?? throw new KeyNotFoundException("Course not found.");

        var enrolledCount = await _context.Enrollments
            .CountAsync(e => e.CourseId == courseId &&
                           (e.Status == EnrollmentStatus.InProgress || e.Status == EnrollmentStatus.Completed), cancellationToken);

        return new ScheduledDeletionStatusDto
        {
            CourseId = course.Id,
            ScheduledDeletionAt = course.ScheduledDeletionAt,
            DeletionReason = course.DeletionReason,
            IsReadOnlyForStudents = course.IsReadOnlyForStudents,
            EnrolledStudentCount = enrolledCount
        };
    }

    private async Task<Course> GetCourseForInstructorAsync(Guid courseId, Guid instructorId, CancellationToken cancellationToken = default)
    {
        var course = await _courseRepo.FirstOrDefaultAsync(
            c => c.Id == courseId && c.DeletedAt == null,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException("Course not found.");

        if (course.CreatedBy != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to modify this course.");

        return course;
    }

    private static string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return string.Empty;

        var slug = title.Trim().ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-").Trim('-');

        while (slug.Contains("--"))
            slug = slug.Replace("--", "-");

        return slug;
    }

    private async Task DeleteContentByItemAsync(SectionItem item, CancellationToken cancellationToken = default)
    {
        switch (item.ItemType)
        {
            case SectionItemType.Video:
                var video = await _context.Videos.FindAsync(new object[] { item.ItemId }, cancellationToken);
                if (video is not null) _context.Videos.Remove(video);
                break;

            case SectionItemType.Document:
                var document = await _context.Documents.FindAsync(new object[] { item.ItemId }, cancellationToken);
                if (document is not null) _context.Documents.Remove(document);
                break;

            case SectionItemType.Quiz:
                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                        .ThenInclude(qn => qn.Options)
                    .FirstOrDefaultAsync(q => q.Id == item.ItemId, cancellationToken);
                if (quiz is not null)
                {
                    foreach (var q in quiz.Questions)
                        _context.Options.RemoveRange(q.Options);

                    _context.Questions.RemoveRange(quiz.Questions);
                    _context.Quizzes.Remove(quiz);
                }
                break;

            case SectionItemType.LiveSession:
                var liveSession = await _context.LiveSessions.FindAsync(new object[] { item.ItemId }, cancellationToken);
                if (liveSession is not null) _context.LiveSessions.Remove(liveSession);
                break;
        }
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
            CreatorName = course.Creator is not null
                ? $"{course.Creator.FirstName} {course.Creator.LastName}".Trim()
                : string.Empty,
            Level = course.Level,
            Language = course.Language,
            Price = course.Price,
            Status = course.Status,
            ImageUrl = course.CourseImageFile is not null
                ? _objectStorage.GetPublicUrl(course.CourseImageFile.Bucket, course.CourseImageFile.FilePath)
                : null,
            CourseImageFileId = course.CourseImageFileId,
            TotalDurationMinutes = course.TotalDurationMinutes,
            EnrollmentCount = course.EnrollmentCount,
            AverageRating = course.AverageRating,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt,
            PublishedAt = course.PublishedAt,
            ScheduledDeletionAt = course.ScheduledDeletionAt,
            DeletionReason = course.DeletionReason,
            IsReadOnlyForStudents = course.IsReadOnlyForStudents,
            RejectionReason = course.RejectionReason,
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
}
