using Athary.Application.Common;
using Athary.Application.DTOs.Admin;
using Athary.Application.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedList<AdminUserListItemDto>>>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminUserService.GetUsersAsync(search, role, isActive, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedList<AdminUserListItemDto>>.SuccessResponse(result));
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<ApiResponse<AdminUserListItemDto>>> GetUserById(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.GetUserByIdAsync(userId, cancellationToken);
        if (result is null)
            return NotFound(ApiResponse<AdminUserListItemDto>.FailureResponse("المستخدم غير موجود."));
        return Ok(ApiResponse<AdminUserListItemDto>.SuccessResponse(result));
    }

    [HttpPatch("{userId:guid}/toggle-active")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleActive(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.ToggleUserActiveAsync(userId, cancellationToken);
        if (!result)
            return NotFound(ApiResponse<bool>.FailureResponse("المستخدم غير موجود."));
        return Ok(ApiResponse<bool>.SuccessResponse(result));
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> DeleteUser(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _adminUserService.DeleteUserAsync(userId, cancellationToken);
        if (!result)
            return NotFound(ApiResponse<bool>.FailureResponse("المستخدم غير موجود."));
        return NoContent();
    }
}
