using Athary.Application.Common;
using Athary.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/public/about")]
[ApiExplorerSettings(GroupName = "Public")]
[Tags("Public - About")]
public class PublicAboutController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PublicAboutController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(Duration = 3600)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAboutInfo(CancellationToken cancellationToken)
    {
        var settings = await _context.SystemSettings
            .Where(s => s.Key.StartsWith("About."))
            .ToListAsync(cancellationToken);

        var getSetting = (string key) => settings.FirstOrDefault(s => s.Key == key)?.Value ?? string.Empty;

        var aboutData = new
        {
            Title = getSetting("About.Title"),
            Description = getSetting("About.Description"),
            Mission = getSetting("About.Mission"),
            Vision = getSetting("About.Vision"),
            Stats = new
            {
                ManuscriptsCount = 200,
                LearnersCount = 10000,
                YearsOfExperience = 14
            }
        };

        return Ok(ApiResponse<object>.SuccessResponse(aboutData));
    }
}
