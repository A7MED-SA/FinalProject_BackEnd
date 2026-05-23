using backend_project.Data;
using backend_project.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_project.Helpers;

public class EnrollmentGuard
{
    private readonly ApplicationDbContext _context;

    public EnrollmentGuard(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Validates if a user has an active enrollment for a given course.
    /// Active means the status is InProgress or Completed.
    /// </summary>
    public async Task<bool> HasActiveEnrollmentAsync(Guid userId, Guid courseId)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .AnyAsync(e => e.UserId == userId 
                        && e.CourseId == courseId 
                        && (e.Status == EnrollmentStatus.InProgress || e.Status == EnrollmentStatus.Completed));
    }

    /// <summary>
    /// Gets the enrollment record if the user has an active enrollment.
    /// Returns null if no active enrollment exists.
    /// </summary>
    public async Task<Enrollment?> GetActiveEnrollmentAsync(Guid userId, Guid courseId)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId 
                                   && e.CourseId == courseId 
                                   && (e.Status == EnrollmentStatus.InProgress || e.Status == EnrollmentStatus.Completed));
    }

    /// <summary>
    /// Checks if a course is in read-only mode for students.
    /// When true, students can view content but cannot interact (no quiz submissions, progress updates, etc.)
    /// </summary>
    public async Task<bool> IsCourseReadOnlyAsync(Guid courseId)
    {
        return await _context.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId && c.DeletedAt == null)
            .Select(c => c.IsReadOnlyForStudents)
            .FirstOrDefaultAsync();
    }
}
