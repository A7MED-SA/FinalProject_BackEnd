using backend_project.DTOs.Profile;
using backend_project.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend_project.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        IProfileService profileService,
        ILogger<ProfileController> logger)
    {
        _profileService = profileService;
        _logger = logger;
    }

    /// <summary>
    /// الحصول على الملف الشخصي للمستخدم الحالي
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = GetUserId();
            var profile = await _profileService.GetProfileAsync(userId);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile");
            return BadRequest(new { error = ex.Message });
        }
    }

    [AllowAnonymous] // أو [Authorize] لو عايز بس المسجلين
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetPublicProfile(Guid userId)
    {
        try
        {
            var profile = await _profileService.GetPublicProfileAsync(userId);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting public profile");
            return BadRequest(new { error = "Failed to load profile" });
        }
    }


    /// <summary>
    /// تحديث بيانات الملف الشخصي
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        try
        {
            var userId = GetUserId();
            var profile = await _profileService.UpdateProfileAsync(userId, dto);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return BadRequest(new { error = ex.Message });
        }
    }

    // --- Profile Picture ---

    /// <summary>
    /// رفع صورة الملف الشخصي
    /// </summary>
    [HttpPost("picture")]
    public async Task<IActionResult> SetProfilePicture([FromBody] SetProfileImageDto dto)
    {
        try
        {
            var userId = GetUserId();
            var profile = await _profileService.SetProfileImageAsync(userId, dto.FileId);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting profile picture");
            return BadRequest(new { error = "Failed to set profile picture" });
        }
    }



    /// <summary>
    /// حذف صورة الملف الشخصي
    /// </summary>
    [HttpDelete("picture")]
    public async Task<IActionResult> DeleteProfilePicture()
    {
        try
        {
            var userId = GetUserId();
            await _profileService.DeleteProfilePictureAsync(userId);
            return Ok(new { message = "Profile picture deleted successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting profile picture");
            return BadRequest(new { error = ex.Message });
        }
    }

    // --- Phone Management ---

    /// <summary>
    /// إضافة رقم هاتف جديد
    /// </summary>
    [HttpPost("phones")]
    public async Task<IActionResult> AddPhone([FromBody] AddPhoneDto dto)
    {
        try
        {
            var userId = GetUserId();
            var phone = await _profileService.AddPhoneAsync(userId, dto);
            return CreatedAtAction(nameof(GetProfile), phone);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding phone");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// حذف رقم هاتف
    /// </summary>
    [HttpDelete("phones/{phoneId}")]
    public async Task<IActionResult> DeletePhone(Guid phoneId)
    {
        try
        {
            var userId = GetUserId();
            await _profileService.DeletePhoneAsync(userId, phoneId);
            return Ok(new { message = "Phone deleted successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting phone");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// تعيين رقم هاتف كافتراضي
    /// </summary>
    [HttpPut("phones/{phoneId}/default")]
    public async Task<IActionResult> SetDefaultPhone(Guid phoneId)
    {
        try
        {
            var userId = GetUserId();
            await _profileService.SetDefaultPhoneAsync(userId, phoneId);
            return Ok(new { message = "Default phone set successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default phone");
            return BadRequest(new { error = ex.Message });
        }
    }

    // --- Address Management ---

    /// <summary>
    /// إضافة عنوان جديد
    /// </summary>
    [HttpPost("addresses")]
    public async Task<IActionResult> AddAddress([FromBody] AddAddressDto dto)
    {
        try
        {
            var userId = GetUserId();
            var address = await _profileService.AddAddressAsync(userId, dto);
            return CreatedAtAction(nameof(GetProfile), address);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding address");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// تحديث عنوان موجود
    /// </summary>
    [HttpPut("addresses/{addressId}")]
    public async Task<IActionResult> UpdateAddress(Guid addressId, [FromBody] UpdateAddressDto dto)
    {
        try
        {
            var userId = GetUserId();
            var address = await _profileService.UpdateAddressAsync(userId, addressId, dto);
            return Ok(address);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating address");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// حذف عنوان
    /// </summary>
    [HttpDelete("addresses/{addressId}")]
    public async Task<IActionResult> DeleteAddress(Guid addressId)
    {
        try
        {
            var userId = GetUserId();
            await _profileService.DeleteAddressAsync(userId, addressId);
            return Ok(new { message = "Address deleted successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting address");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// تعيين عنوان كافتراضي
    /// </summary>
    [HttpPut("addresses/{addressId}/default")]
    public async Task<IActionResult> SetDefaultAddress(Guid addressId)
    {
        try
        {
            var userId = GetUserId();
            await _profileService.SetDefaultAddressAsync(userId, addressId);
            return Ok(new { message = "Default address set successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default address");
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(userIdClaim);
    }
}
