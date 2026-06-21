using Athary.Application.Common;
using Athary.Application.DTOs.Communication;
using Athary.Application.Interfaces.Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers.Communication;

[ApiController]
[Route("api/reports")]
[Authorize]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReportResponse>>> Create([FromBody] CreateReportRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _reportService.CreateAsync(userId, request, cancellationToken);
        return Ok(ApiResponse<ReportResponse>.SuccessResponse(result));
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ReportListResponse>>> GetPending([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _reportService.GetPendingAsync(page, pageSize, cancellationToken);
        return Ok(ApiResponse<ReportListResponse>.SuccessResponse(result));
    }

    [HttpPatch("{id}/resolve")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ReportResponse>>> Resolve(Guid id, [FromBody] ResolveReportRequest request, CancellationToken cancellationToken)
    {
        var adminId = GetUserId();
        var result = await _reportService.ResolveAsync(id, request, adminId, cancellationToken);
        return Ok(ApiResponse<ReportResponse>.SuccessResponse(result));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (claim == null || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        return userId;
    }
}
