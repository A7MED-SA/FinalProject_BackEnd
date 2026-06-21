using Athary.Application.Common;
using Athary.Application.DTOs.Admin;
using Athary.Application.Interfaces.Admin;
using Athary.Domain.Entities;
using Athary.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.Admin;

public sealed class AdminUserService : IAdminUserService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public AdminUserService(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<PagedList<AdminUserListItemDto>> GetUsersAsync(string? search, string? role, bool? isActive, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .Where(u => u.DeletedAt == null)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                u.Email!.ToLower().Contains(term) ||
                (u.FirstName + " " + u.LastName).ToLower().Contains(term));
        }

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(role))
        {
            var roleEntity = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == role, cancellationToken);

            if (roleEntity != null)
            {
                var userIdsInRole = await _context.UserRoles
                    .Where(ur => ur.RoleId == roleEntity.Id)
                    .Select(ur => ur.UserId)
                    .ToListAsync(cancellationToken);

                query = query.Where(u => userIdsInRole.Contains(u.Id));
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userDtos = new List<AdminUserListItemDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new AdminUserListItemDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FullName = user.FullName,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin,
                Roles = roles.ToList()
            });
        }

        return new PagedList<AdminUserListItemDto>(userDtos.AsReadOnly(), totalCount, page, pageSize);
    }

    public async Task<AdminUserListItemDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Where(u => u.DeletedAt == null)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new AdminUserListItemDto
        {
            Id = user.Id,
            Email = user.Email ?? "",
            FullName = user.FullName,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            CreatedAt = user.CreatedAt,
            LastLogin = user.LastLogin,
            Roles = roles.ToList()
        };
    }

    public async Task<bool> ToggleUserActiveAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Where(u => u.DeletedAt == null)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null) return false;

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, cancellationToken);

        if (user == null) return false;

        user.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
