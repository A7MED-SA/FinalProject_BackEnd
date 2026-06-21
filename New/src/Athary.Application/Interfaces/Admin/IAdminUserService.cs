using Athary.Application.Common;
using Athary.Application.DTOs.Admin;

namespace Athary.Application.Interfaces.Admin;

public interface IAdminUserService
{
    Task<PagedList<AdminUserListItemDto>> GetUsersAsync(string? search, string? role, bool? isActive, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<AdminUserListItemDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ToggleUserActiveAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
