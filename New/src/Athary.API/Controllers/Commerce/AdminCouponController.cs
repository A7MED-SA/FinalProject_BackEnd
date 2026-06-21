using Athary.Application.Common;
using Athary.Application.DTOs.Commerce;
using Athary.Application.Interfaces.Commerce;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers.Commerce;

[ApiController]
[Route("api/admin/coupons")]
[Authorize(Roles = "Admin")]
public sealed class AdminCouponController : ControllerBase
{
    private readonly ICouponService _couponService;

    public AdminCouponController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CouponResponseDto>>>> GetAll([FromQuery] bool? isActive, CancellationToken cancellationToken)
    {
        var result = await _couponService.GetAllCouponsAsync(isActive, cancellationToken);
        return Ok(ApiResponse<IEnumerable<CouponResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{couponId}")]
    public async Task<ActionResult<ApiResponse<CouponResponseDto>>> GetById(Guid couponId, CancellationToken cancellationToken)
    {
        var result = await _couponService.GetCouponByIdAsync(couponId, cancellationToken);
        if (result is null)
            return NotFound(ApiResponse<CouponResponseDto>.FailureResponse("الكوبون غير موجود."));
        return Ok(ApiResponse<CouponResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CouponResponseDto>>> Create([FromBody] CreateCouponDto dto, CancellationToken cancellationToken)
    {
        var adminId = GetUserId();
        var result = await _couponService.CreateCouponAsync(dto, adminId, cancellationToken);
        return Ok(ApiResponse<CouponResponseDto>.SuccessResponse(result));
    }

    [HttpPut("{couponId}")]
    public async Task<ActionResult<ApiResponse<CouponResponseDto>>> Update(Guid couponId, [FromBody] CreateCouponDto dto, CancellationToken cancellationToken)
    {
        var result = await _couponService.UpdateCouponAsync(couponId, dto, cancellationToken);
        if (result is null)
            return NotFound(ApiResponse<CouponResponseDto>.FailureResponse("الكوبون غير موجود."));
        return Ok(ApiResponse<CouponResponseDto>.SuccessResponse(result));
    }

    [HttpPatch("{couponId}/toggle")]
    public async Task<ActionResult<ApiResponse<bool>>> Toggle(Guid couponId, CancellationToken cancellationToken)
    {
        var result = await _couponService.ToggleCouponAsync(couponId, cancellationToken);
        if (!result)
            return NotFound(ApiResponse<bool>.FailureResponse("الكوبون غير موجود."));
        return Ok(ApiResponse<bool>.SuccessResponse(result));
    }

    [HttpDelete("{couponId}")]
    public async Task<IActionResult> Delete(Guid couponId, CancellationToken cancellationToken)
    {
        var result = await _couponService.DeleteCouponAsync(couponId, cancellationToken);
        if (!result)
            return NotFound(ApiResponse<bool>.FailureResponse("الكوبون غير موجود."));
        return NoContent();
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (claim == null || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        return userId;
    }
}
