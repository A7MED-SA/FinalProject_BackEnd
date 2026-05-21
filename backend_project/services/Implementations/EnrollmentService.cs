using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.Enrollment;
using backend_project.DTOs.ContentProgress;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class EnrollmentService : IEnrollmentService
{
    private readonly ApplicationDbContext _context;

    public EnrollmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EnrollmentResponseDto> EnrollUserAsync(CreateEnrollmentDto createDto)
    {
        // Check if course exists and is published
        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == createDto.CourseId);

        if (course == null)
            throw new Exception("Course not found.");

        if (course.Status != CourseStatus.Published)
            throw new Exception("Cannot enroll in an unpublished course.");

        // Check for duplicate enrollment
        bool alreadyEnrolled = await _context.Enrollments
            .AsNoTracking()
            .AnyAsync(e => e.UserId == createDto.UserId && e.CourseId == createDto.CourseId);

        if (alreadyEnrolled)
            throw new Exception("User is already enrolled in this course.");

        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            UserId = createDto.UserId,
            CourseId = createDto.CourseId,
            Source = createDto.Source,
            EnrolledAt = DateTime.UtcNow,
            Status = EnrollmentStatus.InProgress,
            ProgressPercentage = 0,
            IsRefunded = false
        };

        _context.Enrollments.Add(enrollment);

        // Increment course enrollment count
        // Note: we need to use a tracked entity for course to update its enrollment count, or just update directly
        await _context.Courses
            .Where(c => c.Id == createDto.CourseId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.EnrollmentCount, c => c.EnrollmentCount + 1));

        await _context.SaveChangesAsync();

        return MapToResponseDto(enrollment, course.Title);
    }

    public async Task<EnrollmentDetailDto> GetEnrollmentDetailsAsync(Guid enrollmentId, Guid userId)
    {
        var enrollment = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .Include(e => e.ContentProgresses)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.UserId == userId);

        if (enrollment == null)
            throw new Exception("Enrollment not found or unauthorized.");

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

    public async Task<IEnumerable<EnrollmentResponseDto>> GetUserEnrollmentsAsync(Guid userId)
    {
        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

        return enrollments.Select(e => MapToResponseDto(e, e.Course.Title));
    }

    public async Task<IEnumerable<EnrollmentResponseDto>> GetCourseEnrollmentsAsync(Guid courseId)
    {
        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .Where(e => e.CourseId == courseId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

        return enrollments.Select(e => MapToResponseDto(e, e.Course.Title));
    }

    public async Task<bool> UpdateEnrollmentStatusAsync(Guid enrollmentId, EnrollmentStatus status)
    {
        var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
        if (enrollment == null)
            return false;

        enrollment.Status = status;
        
        if (status == EnrollmentStatus.Completed && !enrollment.CompletedAt.HasValue)
        {
            enrollment.CompletedAt = DateTime.UtcNow;
        }
        else if (status == EnrollmentStatus.Refunded)
        {
            enrollment.IsRefunded = true;
            // Also decrement the course enrollment count
            await _context.Courses
                .Where(c => c.Id == enrollment.CourseId)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.EnrollmentCount, c => c.EnrollmentCount - 1));
        }

        await _context.SaveChangesAsync();
        return true;
    }

    private EnrollmentResponseDto MapToResponseDto(Enrollment e, string courseTitle)
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
