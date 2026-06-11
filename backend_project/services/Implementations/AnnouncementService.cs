using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.Communication;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class AnnouncementService : IAnnouncementService
{
    private readonly ApplicationDbContext _context;

    public AnnouncementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AnnouncementResponse> CreateAsync(Guid userId, CreateAnnouncementRequest request)
    {
        var isAdmin = await IsUserInRoleAsync(userId, "Admin");
        var isInstructor = await IsUserInRoleAsync(userId, "Instructor");

        if (!isAdmin && !isInstructor)
            throw new InvalidOperationException("Only admins and instructors can create announcements.");

        if (!Enum.TryParse<AnnouncementTarget>(request.Target, out var target))
            throw new InvalidOperationException("Invalid target audience.");

        if (target == AnnouncementTarget.SpecificCourse && request.CourseId == null)
            throw new InvalidOperationException("CourseId is required when target is SpecificCourse.");

        if (target == AnnouncementTarget.SpecificCourse && isInstructor)
        {
            var ownsCourse = await _context.Courses.AnyAsync(c => c.Id == request.CourseId && c.CreatedBy == userId);
            if (!ownsCourse)
                throw new InvalidOperationException("You can only create announcements for your own courses.");
        }

        if (target != AnnouncementTarget.SpecificCourse && !isAdmin)
            throw new InvalidOperationException("Instructors can only create course-specific announcements.");

        var announcement = new Announcement
        {
            Title = request.Title,
            Content = request.Content,
            Target = target,
            CourseId = target == AnnouncementTarget.SpecificCourse ? request.CourseId : null,
            CreatedBy = userId,
            IsActive = true,
            PublishedAt = DateTime.UtcNow
        };

        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync();

        return MapToResponse(announcement);
    }

    public async Task<AnnouncementResponse> UpdateAsync(Guid id, Guid userId, UpdateAnnouncementRequest request)
    {
        var announcement = await _context.Announcements.FindAsync(id)
            ?? throw new KeyNotFoundException("Announcement not found.");

        var isAdmin = await IsUserInRoleAsync(userId, "Admin");
        var isInstructor = await IsUserInRoleAsync(userId, "Instructor");

        if (announcement.CreatedBy != userId && !isAdmin)
            throw new InvalidOperationException("You can only update your own announcements.");

        if (!Enum.TryParse<AnnouncementTarget>(request.Target, out var target))
            throw new InvalidOperationException("Invalid target audience.");

        announcement.Title = request.Title;
        announcement.Content = request.Content;
        announcement.Target = target;
        announcement.CourseId = target == AnnouncementTarget.SpecificCourse ? request.CourseId : null;
        announcement.IsActive = request.IsActive;

        await _context.SaveChangesAsync();
        return MapToResponse(announcement);
    }

    public async Task DeactivateAsync(Guid id, Guid userId)
    {
        var announcement = await _context.Announcements.FindAsync(id)
            ?? throw new KeyNotFoundException("Announcement not found.");

        var isAdmin = await IsUserInRoleAsync(userId, "Admin");
        if (announcement.CreatedBy != userId && !isAdmin)
            throw new InvalidOperationException("You can only deactivate your own announcements.");

        announcement.IsActive = false;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var announcement = await _context.Announcements.FindAsync(id)
            ?? throw new KeyNotFoundException("Announcement not found.");

        var isAdmin = await IsUserInRoleAsync(userId, "Admin");
        if (announcement.CreatedBy != userId && !isAdmin)
            throw new InvalidOperationException("You can only delete your own announcements.");

        _context.Announcements.Remove(announcement);
        await _context.SaveChangesAsync();
    }

    public async Task<AnnouncementListResponse> GetFeedAsync(Guid userId, int page, int pageSize)
    {
        var query = _context.Announcements
            .Include(a => a.Creator)
            .Where(a => a.IsActive);

        var isStudent = await IsUserInRoleAsync(userId, "Student");
        if (isStudent)
        {
            var enrolledCourseIds = await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseId)
                .ToListAsync();

            query = query.Where(a =>
                a.Target == AnnouncementTarget.All ||
                a.Target == AnnouncementTarget.Students ||
                (a.Target == AnnouncementTarget.SpecificCourse && a.CourseId != null && enrolledCourseIds.Contains(a.CourseId.Value)));
        }
        else
        {
            var isInstructor = await IsUserInRoleAsync(userId, "Instructor");
            if (isInstructor)
            {
                var instructorCourseIds = await _context.Courses
                    .Where(c => c.CreatedBy == userId)
                    .Select(c => c.Id)
                    .ToListAsync();

                query = query.Where(a =>
                    a.Target == AnnouncementTarget.All ||
                    a.Target == AnnouncementTarget.Teachers ||
                    (a.Target == AnnouncementTarget.SpecificCourse && a.CourseId != null && instructorCourseIds.Contains(a.CourseId.Value)));
            }
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new AnnouncementListResponse
        {
            Items = items.Select(MapToResponse),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private async Task<bool> IsUserInRoleAsync(Guid userId, string roleName)
    {
        var roleId = await _context.Roles
            .Where(r => r.Name == roleName)
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        if (roleId == Guid.Empty) return false;

        return await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
    }

    private static AnnouncementResponse MapToResponse(Announcement a)
    {
        return new AnnouncementResponse
        {
            Id = a.Id,
            Title = a.Title,
            Content = a.Content,
            Target = a.Target.ToString(),
            CourseId = a.CourseId,
            CreatedBy = a.CreatedBy,
            CreatedByName = $"{a.Creator.FirstName} {a.Creator.LastName}",
            IsActive = a.IsActive,
            PublishedAt = a.PublishedAt
        };
    }
}
