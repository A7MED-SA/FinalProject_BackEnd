using Athary.Application.Common;
using Athary.Application.DTOs.Profile;
using Athary.Application.Interfaces.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/public/instructors")]
public class PublicInstructorController : ControllerBase
{
    private readonly IProfileService _profileService;

    public PublicInstructorController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PublicProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var instructor = await _profileService.GetBySlugAsync(slug, cancellationToken);

        if (instructor is null)
            return NotFound(ApiResponse<object>.FailureResponse("Instructor not found"));

        return Ok(ApiResponse<PublicProfileDto>.SuccessResponse(instructor));
    }

    [HttpGet("check-slug")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckSlugAvailability([FromQuery] string slug, CancellationToken cancellationToken)
    {
        var isAvailable = await _profileService.IsSlugAvailableAsync(slug, cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResponse(isAvailable));
    }

    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<PublicProfileDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(ApiResponse<List<PublicProfileDto>>.SuccessResponse(new List<PublicProfileDto>()));

        var instructors = await _profileService.SearchBySlugAsync(q, cancellationToken);
        return Ok(ApiResponse<List<PublicProfileDto>>.SuccessResponse(instructors));
    }
}
