using Athary.Application.Common;
using Athary.Application.DTOs.Public;
using Athary.Application.Interfaces.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/public")]
[ApiExplorerSettings(GroupName = "Public")]
[Tags("Public - Landing")]
public class PublicController : ControllerBase
{
    private readonly IPublicService _publicService;

    public PublicController(IPublicService publicService)
    {
        _publicService = publicService;
    }

    [HttpGet("landing")]
    [AllowAnonymous]
    [ResponseCache(Duration = 300)]
    [ProducesResponseType(typeof(ApiResponse<LandingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLanding(CancellationToken cancellationToken)
    {
        var data = await _publicService.GetLandingDataAsync(cancellationToken);
        return Ok(ApiResponse<LandingDto>.SuccessResponse(data));
    }

    [HttpGet("stats")]
    [AllowAnonymous]
    [ResponseCache(Duration = 300)]
    [ProducesResponseType(typeof(ApiResponse<LandingStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var data = await _publicService.GetLandingDataAsync(cancellationToken);
        return Ok(ApiResponse<LandingStatsDto>.SuccessResponse(data.Stats));
    }
}
