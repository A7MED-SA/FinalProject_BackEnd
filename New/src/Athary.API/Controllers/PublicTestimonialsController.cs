using System.Security.Claims;
using Athary.Application.Common;
using Athary.Application.DTOs.Public;
using Athary.Application.Interfaces.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/public/testimonials")]
[ApiExplorerSettings(GroupName = "Public")]
[Tags("Public - Testimonials")]
public class PublicTestimonialsController : ControllerBase
{
    private readonly ITestimonialService _testimonialService;

    public PublicTestimonialsController(ITestimonialService testimonialService)
    {
        _testimonialService = testimonialService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(Duration = 600)]
    [ProducesResponseType(typeof(ApiResponse<List<TestimonialDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? minRating = null,
        CancellationToken cancellationToken = default)
    {
        var testimonials = await _testimonialService.GetApprovedAsync(page, pageSize, minRating, cancellationToken);
        return Ok(ApiResponse<List<TestimonialDto>>.SuccessResponse(testimonials));
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<TestimonialDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTestimonialDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var testimonial = await _testimonialService.CreateAsync(userId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetAll), ApiResponse<TestimonialDto>.SuccessResponse(testimonial));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null ? Guid.Parse(claim.Value) : throw new UnauthorizedAccessException();
    }
}
