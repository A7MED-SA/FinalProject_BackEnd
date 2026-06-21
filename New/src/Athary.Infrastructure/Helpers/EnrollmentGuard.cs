using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Helpers;

public sealed class EnrollmentGuard
{
    private readonly ApplicationDbContext _context;

    public EnrollmentGuard(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasActiveEnrollmentAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .AnyAsync(e => e.UserId == userId
                        && e.CourseId == courseId
                        && (e.Status == EnrollmentStatus.InProgress || e.Status == EnrollmentStatus.Completed),
                cancellationToken);
    }

    public async Task<Enrollment?> GetActiveEnrollmentAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId
                                   && e.CourseId == courseId
                                   && (e.Status == EnrollmentStatus.InProgress || e.Status == EnrollmentStatus.Completed),
                cancellationToken);
    }

    public async Task<bool> IsCourseReadOnlyAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId && c.DeletedAt == null)
            .Select(c => c.IsReadOnlyForStudents)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
