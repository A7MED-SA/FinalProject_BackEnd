using Athary.Application.Common;
using Athary.Application.DTOs.Public;
using Athary.Application.Interfaces.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/public/legal")]
[ApiExplorerSettings(GroupName = "Public")]
[Tags("Public - Legal")]
public class PublicLegalController : ControllerBase
{
    private readonly ILegalPageService _legalPageService;

    private static readonly string[] ValidTypes = new[] { "privacy", "terms", "refund" };

    public PublicLegalController(ILegalPageService legalPageService)
    {
        _legalPageService = legalPageService;
    }

    [HttpGet("{type}")]
    [AllowAnonymous]
    [ResponseCache(Duration = 3600)]
    [ProducesResponseType(typeof(ApiResponse<LegalPageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLegalContent(string type, CancellationToken cancellationToken)
    {
        if (!ValidTypes.Contains(type.ToLower()))
            return BadRequest(ApiResponse<object>.FailureResponse("Invalid legal page type"));

        var content = await _legalPageService.GetByTypeAsync(type.ToLower(), cancellationToken);

        if (content is null)
            return NotFound(ApiResponse<object>.FailureResponse("Legal page not found"));

        return Ok(ApiResponse<LegalPageDto>.SuccessResponse(content));
    }
}
