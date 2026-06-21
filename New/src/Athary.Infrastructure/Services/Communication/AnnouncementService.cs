using Athary.Application.DTOs.Communication;
using Athary.Application.Interfaces.Communication;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Data;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.Communication;

public sealed class AnnouncementService : IAnnouncementService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AnnouncementService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AnnouncementResponse> CreateAsync(Guid userId, CreateAnnouncementRequest request, CancellationToken cancellationToken = default)
    {
        var isAdmin = await IsUserInRoleAsync(userId, "Admin", cancellationToken);
        var isInstructor = await IsUserInRoleAsync(userId, "Instructor", cancellationToken);

        if (!isAdmin && !isInstructor)
            throw new InvalidOperationException("فقط الأدمن والمدرسين يمكنهم إنشاء إعلانات.");

        if (!Enum.TryParse<AnnouncementTarget>(request.Target, true, out var target))
            throw new InvalidOperationException("الجمهور المستهدف غير صالح.");

        if (target == AnnouncementTarget.SpecificCourse && request.CourseId is null)
            throw new InvalidOperationException("معرف الدورة مطلوب عند اختيار دورة محددة.");

        if (target == AnnouncementTarget.SpecificCourse && isInstructor)
        {
            var ownsCourse = await _context.Courses
                .AnyAsync(c => c.Id == request.CourseId && c.CreatedBy == userId, cancellationToken);

            if (!ownsCourse)
                throw new InvalidOperationException("يمكنك فقط إنشاء إعلانات لدوراتك الخاصة.");
        }

        if (target != AnnouncementTarget.SpecificCourse && !isAdmin)
            throw new InvalidOperationException("المدرسين يمكنهم فقط إنشاء إعلانات خاصة بدوراتهم.");

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
        await _context.SaveChangesAsync(cancellationToken);

        await _context.Entry(announcement).Reference(a => a.Creator).LoadAsync(cancellationToken);
        return _mapper.Map<AnnouncementResponse>(announcement);
    }

    public async Task<AnnouncementResponse> UpdateAsync(Guid id, Guid userId, UpdateAnnouncementRequest request, CancellationToken cancellationToken = default)
    {
        var announcement = await _context.Announcements
            .Include(a => a.Creator)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("الإعلان غير موجود.");

        var isAdmin = await IsUserInRoleAsync(userId, "Admin", cancellationToken);

        if (announcement.CreatedBy != userId && !isAdmin)
            throw new InvalidOperationException("يمكنك فقط تعديل إعلاناتك الخاصة.");

        if (!Enum.TryParse<AnnouncementTarget>(request.Target, true, out var target))
            throw new InvalidOperationException("الجمهور المستهدف غير صالح.");

        announcement.Title = request.Title;
        announcement.Content = request.Content;
        announcement.Target = target;
        announcement.CourseId = target == AnnouncementTarget.SpecificCourse ? request.CourseId : null;
        announcement.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AnnouncementResponse>(announcement);
    }

    public async Task DeactivateAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var announcement = await _context.Announcements
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("الإعلان غير موجود.");

        var isAdmin = await IsUserInRoleAsync(userId, "Admin", cancellationToken);

        if (announcement.CreatedBy != userId && !isAdmin)
            throw new InvalidOperationException("يمكنك فقط إلغاء تفعيل إعلاناتك الخاصة.");

        announcement.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var announcement = await _context.Announcements
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("الإعلان غير موجود.");

        var isAdmin = await IsUserInRoleAsync(userId, "Admin", cancellationToken);

        if (announcement.CreatedBy != userId && !isAdmin)
            throw new InvalidOperationException("يمكنك فقط حذف إعلاناتك الخاصة.");

        _context.Announcements.Remove(announcement);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<AnnouncementListResponse> GetFeedAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _context.Announcements
            .Include(a => a.Creator)
            .Where(a => a.IsActive);

        var isStudent = await IsUserInRoleAsync(userId, "Student", cancellationToken);

        if (isStudent)
        {
            var enrolledCourseIds = await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseId)
                .ToListAsync(cancellationToken);

            query = query.Where(a =>
                a.Target == AnnouncementTarget.All ||
                a.Target == AnnouncementTarget.Students ||
                (a.Target == AnnouncementTarget.SpecificCourse && a.CourseId != null && enrolledCourseIds.Contains(a.CourseId.Value)));
        }
        else
        {
            var isInstructor = await IsUserInRoleAsync(userId, "Instructor", cancellationToken);

            if (isInstructor)
            {
                var instructorCourseIds = await _context.Courses
                    .Where(c => c.CreatedBy == userId)
                    .Select(c => c.Id)
                    .ToListAsync(cancellationToken);

                query = query.Where(a =>
                    a.Target == AnnouncementTarget.All ||
                    a.Target == AnnouncementTarget.Instructors ||
                    (a.Target == AnnouncementTarget.SpecificCourse && a.CourseId != null && instructorCourseIds.Contains(a.CourseId.Value)));
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new AnnouncementListResponse
        {
            Items = _mapper.Map<List<AnnouncementResponse>>(items),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private async Task<bool> IsUserInRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        var roleId = await _context.Roles
            .Where(r => r.Name == roleName)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (roleId == Guid.Empty) return false;

        return await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);
    }

}
