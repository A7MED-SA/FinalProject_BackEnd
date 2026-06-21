using System.Security.Claims;
using Athary.Application.Common;
using Athary.Application.DTOs.Profile;
using Athary.Application.Interfaces.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> GetProfile(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var profile = await _profileService.GetProfileAsync(userId, cancellationToken);
        return Ok(ApiResponse<ProfileDto>.SuccessResponse(profile));
    }

    [AllowAnonymous]
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<ApiResponse<PublicProfileDto>>> GetPublicProfile(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await _profileService.GetPublicProfileAsync(userId, cancellationToken);
        return Ok(ApiResponse<PublicProfileDto>.SuccessResponse(profile));
    }

    [Authorize]
    [HttpPut]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> UpdateProfile([FromBody] UpdateProfileDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var profile = await _profileService.UpdateProfileAsync(userId, dto, cancellationToken);
        return Ok(ApiResponse<ProfileDto>.SuccessResponse(profile));
    }

    [Authorize]
    [HttpPost("picture")]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> SetProfilePicture([FromBody] SetProfileImageDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var profile = await _profileService.SetProfileImageAsync(userId, dto.FileId, cancellationToken);
        return Ok(ApiResponse<ProfileDto>.SuccessResponse(profile));
    }

    [Authorize]
    [HttpDelete("picture")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteProfilePicture(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _profileService.DeleteProfilePictureAsync(userId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Profile picture deleted successfully"));
    }

    [Authorize]
    [HttpPost("phones")]
    public async Task<ActionResult<ApiResponse<PhoneDto>>> AddPhone([FromBody] AddPhoneDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var phone = await _profileService.AddPhoneAsync(userId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetProfile), null, ApiResponse<PhoneDto>.SuccessResponse(phone));
    }

    [Authorize]
    [HttpDelete("phones/{phoneId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeletePhone(Guid phoneId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _profileService.DeletePhoneAsync(userId, phoneId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Phone deleted successfully"));
    }

    [Authorize]
    [HttpPut("phones/{phoneId:guid}/default")]
    public async Task<ActionResult<ApiResponse<object>>> SetDefaultPhone(Guid phoneId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _profileService.SetDefaultPhoneAsync(userId, phoneId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Default phone set successfully"));
    }

    [Authorize]
    [HttpPost("addresses")]
    public async Task<ActionResult<ApiResponse<AddressDto>>> AddAddress([FromBody] AddAddressDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var address = await _profileService.AddAddressAsync(userId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetProfile), null, ApiResponse<AddressDto>.SuccessResponse(address));
    }

    [Authorize]
    [HttpPut("addresses/{addressId:guid}")]
    public async Task<ActionResult<ApiResponse<AddressDto>>> UpdateAddress(Guid addressId, [FromBody] UpdateAddressDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var address = await _profileService.UpdateAddressAsync(userId, addressId, dto, cancellationToken);
        return Ok(ApiResponse<AddressDto>.SuccessResponse(address));
    }

    [Authorize]
    [HttpDelete("addresses/{addressId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAddress(Guid addressId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _profileService.DeleteAddressAsync(userId, addressId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Address deleted successfully"));
    }

    [Authorize]
    [HttpPut("addresses/{addressId:guid}/default")]
    public async Task<ActionResult<ApiResponse<object>>> SetDefaultAddress(Guid addressId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _profileService.SetDefaultAddressAsync(userId, addressId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Default address set successfully"));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(userIdClaim);
    }
}
