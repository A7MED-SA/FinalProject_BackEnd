using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Domain.Interfaces;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Athary.Infrastructure.Services.Courses;

public sealed class EnrollmentService : IEnrollmentService
{
    private readonly ApplicationDbContext _context;
    private readonly IRepository<Enrollment> _enrollmentRepo;
    private readonly IRepository<Course> _courseRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(
        ApplicationDbContext context,
        IRepository<Enrollment> enrollmentRepo,
        IRepository<Course> courseRepo,
        IUnitOfWork unitOfWork,
        ILogger<EnrollmentService> logger)
    {
        _context = context;
        _enrollmentRepo = enrollmentRepo;
        _courseRepo = courseRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<EnrollmentResponseDto> EnrollUserAsync(CreateEnrollmentDto createDto, CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == createDto.CourseId, cancellationToken)
            ?? throw new InvalidOperationException("Course not found.");

        if (course.Status != CourseStatus.Published)
            throw new InvalidOperationException("Cannot enroll in an unpublished course.");

        bool alreadyEnrolled = await _context.Enrollments
            .AsNoTracking()
            .AnyAsync(e => e.UserId == createDto.UserId && e.CourseId == createDto.CourseId, cancellationToken);

        if (alreadyEnrolled)
            throw new InvalidOperationException("User is already enrolled in this course.");

        var enrollment = new Enrollment
        {
            UserId = createDto.UserId,
            CourseId = createDto.CourseId,
            Source = createDto.Source
        };

        await _enrollmentRepo.AddAsync(enrollment, cancellationToken);

        await _context.Courses
            .Where(c => c.Id == createDto.CourseId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.EnrollmentCount, c => c.EnrollmentCount + 1), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} enrolled in course {CourseId}", createDto.UserId, createDto.CourseId);

        return MapToResponseDto(enrollment, course.Title);
    }

    public async Task<EnrollmentDetailDto> GetEnrollmentDetailsAsync(Guid enrollmentId, Guid userId, CancellationToken cancellationToken = default)
    {
        var enrollment = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .Include(e => e.ContentProgresses)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("Enrollment not found or unauthorized.");

        return new EnrollmentDetailDto
        {
            Id = enrollment.Id,
            UserId = enrollment.UserId,
            CourseId = enrollment.CourseId,
            CourseTitle = enrollment.Course.Title,
            EnrolledAt = enrollment.EnrolledAt,
            Status = enrollment.Status,
            ProgressPercentage = enrollment.ProgressPercentage,
            CompletedAt = enrollment.CompletedAt,
            LastAccessedAt = enrollment.LastAccessedAt,
            Source = enrollment.Source,
            AccessExpiresAt = enrollment.AccessExpiresAt,
            IsRefunded = enrollment.IsRefunded,
            Progresses = enrollment.ContentProgresses.Select(cp => new ContentProgressDto
            {
                Id = cp.Id,
                EnrollmentId = cp.EnrollmentId,
                ContentType = cp.ContentType,
                ContentId = cp.ContentId,
                IsCompleted = cp.IsCompleted,
                WatchTimeSeconds = cp.WatchTimeSeconds,
                AttemptsCount = cp.AttemptsCount,
                CompletionPercentage = cp.CompletionPercentage,
                Metadata = cp.Metadata,
                LastAccessedAt = cp.LastAccessedAt,
                CompletedAt = cp.CompletedAt
            })
        };
    }

    public async Task<IEnumerable<EnrollmentResponseDto>> GetUserEnrollmentsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync(cancellationToken);

        return enrollments.Select(e => MapToResponseDto(e, e.Course.Title));
    }

    public async Task<IEnumerable<EnrollmentResponseDto>> GetCourseEnrollmentsAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .Where(e => e.CourseId == courseId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync(cancellationToken);

        return enrollments.Select(e => MapToResponseDto(e, e.Course.Title));
    }

    public async Task<bool> UpdateEnrollmentStatusAsync(Guid enrollmentId, EnrollmentStatus status, CancellationToken cancellationToken = default)
    {
        var enrollment = await _enrollmentRepo.GetByIdAsync(enrollmentId, cancellationToken);
        if (enrollment is null)
            return false;

        enrollment.Status = status;

        if (status == EnrollmentStatus.Completed && !enrollment.CompletedAt.HasValue)
            enrollment.CompletedAt = DateTime.UtcNow;
        else if (status == EnrollmentStatus.Refunded)
        {
            enrollment.IsRefunded = true;
            await _context.Courses
                .Where(c => c.Id == enrollment.CourseId)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.EnrollmentCount, c => c.EnrollmentCount - 1), cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static EnrollmentResponseDto MapToResponseDto(Enrollment e, string courseTitle)
    {
        return new EnrollmentResponseDto
        {
            Id = e.Id,
            UserId = e.UserId,
            CourseId = e.CourseId,
            CourseTitle = courseTitle,
            EnrolledAt = e.EnrolledAt,
            Status = e.Status,
            ProgressPercentage = e.ProgressPercentage,
            CompletedAt = e.CompletedAt,
            LastAccessedAt = e.LastAccessedAt,
            Source = e.Source,
            AccessExpiresAt = e.AccessExpiresAt,
            IsRefunded = e.IsRefunded
        };
    }
}
