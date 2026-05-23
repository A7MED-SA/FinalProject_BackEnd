using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.DTOs.Coupon;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/admin/coupons")]
[Authorize(Roles = "Admin")]
public class AdminCouponController : ControllerBase
{
    private readonly ICouponService _couponService;

    public AdminCouponController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null)
    {
        var coupons = await _couponService.GetAllCouponsAsync(isActive);
        return Ok(ApiResponse<IEnumerable<CouponResponseDto>>.SuccessResponse(coupons));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var coupon = await _couponService.GetCouponByIdAsync(id);
        if (coupon == null)
            return NotFound(ApiResponse<object>.FailureResponse("Coupon not found."));

        return Ok(ApiResponse<CouponResponseDto>.SuccessResponse(coupon));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCouponDto dto)
    {
        try
        {
            var claimsUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userId = Guid.Parse(claimsUserId!);
            var coupon = await _couponService.CreateCouponAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = coupon.Id }, ApiResponse<CouponResponseDto>.SuccessResponse(coupon, "Coupon created."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateCouponDto dto)
    {
        try
        {
            var coupon = await _couponService.UpdateCouponAsync(id, dto);
            if (coupon == null)
                return NotFound(ApiResponse<object>.FailureResponse("Coupon not found."));

            return Ok(ApiResponse<CouponResponseDto>.SuccessResponse(coupon, "Coupon updated."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle(Guid id)
    {
        var result = await _couponService.ToggleCouponAsync(id);
        if (!result)
            return NotFound(ApiResponse<object>.FailureResponse("Coupon not found."));

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Coupon toggled."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _couponService.DeleteCouponAsync(id);
        if (!result)
            return NotFound(ApiResponse<object>.FailureResponse("Coupon not found."));

        return NoContent();
    }
}
