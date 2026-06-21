using Athary.Application.Common;
using Athary.Application.DTOs.Commerce;
using Athary.Application.Interfaces.Commerce;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers.Commerce;

[ApiController]
[Route("api/coupons")]
public sealed class CouponsController : ControllerBase
{
    private readonly ICouponService _couponService;

    public CouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpPost("validate")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ValidateCouponResponse>>> ValidateCoupon([FromBody] ValidateCouponRequest request, CancellationToken cancellationToken)
    {
        var result = await _couponService.ValidateCouponAsync(request.Code, request.CartTotal, request.CourseIds, cancellationToken);
        return Ok(ApiResponse<ValidateCouponResponse>.SuccessResponse(result));
    }
}
