using Athary.Application.Common;
using Athary.Application.DTOs.ActivityLog;
using Athary.Application.Interfaces.Authentication;
using Athary.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers.Communication;

[ApiController]
[Route("api/activity-logs")]
[Authorize(Roles = "Admin")]
public class ActivityLogsController : ControllerBase
{
    private readonly IActivityLogService _activityLogService;

    public ActivityLogsController(IActivityLogService activityLogService)
    {
        _activityLogService = activityLogService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ActivityLogListResponse>>> GetLogs(
        [FromQuery] ActivityLogFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _activityLogService.GetLogsAsync(
            filter.UserId,
            filter.Action,
            filter.EntityType,
            filter.DateFrom,
            filter.DateTo,
            filter.IpAddress,
            filter.Page,
            filter.PageSize,
            cancellationToken);

        var response = new ActivityLogListResponse
        {
            Items = items.Select(MapToResponse),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };

        return Ok(ApiResponse<ActivityLogListResponse>.SuccessResponse(response));
    }

    private static ActivityLogResponse MapToResponse(ActivityLog log)
    {
        return new ActivityLogResponse
        {
            Id = log.Id,
            UserId = log.UserId,
            UserName = log.User?.FullName ?? string.Empty,
            Action = log.Action,
            EntityType = log.EntityType.ToString(),
            EntityId = log.EntityId,
            Details = log.Details,
            IpAddress = log.IpAddress,
            CreatedAt = log.CreatedAt
        };
    }
}
