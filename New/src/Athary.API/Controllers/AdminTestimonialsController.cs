using Athary.Application.Common;
using Athary.Application.DTOs.Public;
using Athary.Application.Interfaces.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/admin/testimonials")]
[Authorize(Roles = "Admin")]
[ApiExplorerSettings(GroupName = "Admin")]
[Tags("Admin - Testimonials")]
public class AdminTestimonialsController : ControllerBase
{
    private readonly ITestimonialService _testimonialService;

    public AdminTestimonialsController(ITestimonialService testimonialService)
    {
        _testimonialService = testimonialService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<TestimonialDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isApproved = null, CancellationToken cancellationToken = default)
    {
        var testimonials = await _testimonialService.GetAllAsync(isApproved, cancellationToken);
        return Ok(ApiResponse<List<TestimonialDto>>.SuccessResponse(testimonials));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TestimonialDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTestimonialDto dto, CancellationToken cancellationToken)
    {
        var testimonial = await _testimonialService.UpdateAdminAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<TestimonialDto>.SuccessResponse(testimonial));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _testimonialService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        await _testimonialService.ApproveAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Testimonial approved"));
    }

    [HttpPatch("{id:guid}/flag")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Flag(Guid id, [FromBody] string reason, CancellationToken cancellationToken)
    {
        await _testimonialService.FlagAsync(id, reason, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Testimonial flagged"));
    }

    [HttpPut("reorder")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reorder([FromBody] List<ReorderItemDto> items, CancellationToken cancellationToken)
    {
        await _testimonialService.ReorderAsync(items, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Testimonials reordered"));
    }
}
