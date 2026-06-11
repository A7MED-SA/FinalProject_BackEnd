using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using backend_project.Data;
using backend_project.DTOs;
using backend_project.DTOs.Communication;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers.Communication;

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
    public async Task<IActionResult> GetLogs([FromQuery] ActivityLogFilterRequest filter)
    {
        if (filter.PageSize > 100)
            filter.PageSize = 100;

        var (items, totalCount) = await _activityLogService.GetLogsAsync(
            filter.UserId,
            filter.Action,
            filter.EntityType,
            filter.DateFrom,
            filter.DateTo,
            filter.IpAddress,
            filter.Page,
            filter.PageSize);

        var response = new ActivityLogListResponse
        {
            Items = items.Select(l => new ActivityLogResponse
            {
                Id = l.Id,
                UserId = l.UserId,
                UserName = l.User?.UserName ?? "Deleted User",
                Action = l.Action,
                EntityType = l.EntityType.ToString(),
                EntityId = l.EntityId,
                Details = l.Details,
                IpAddress = l.IpAddress,
                CreatedAt = l.CreatedAt
            }),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };

        return Ok(ApiResponse<ActivityLogListResponse>.SuccessResponse(response));
    }
}
