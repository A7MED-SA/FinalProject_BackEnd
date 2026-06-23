using Athary.Application.Common;
using Athary.Application.DTOs.Contact;
using Athary.Application.Interfaces.Contact;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/public/contact")]
[ApiExplorerSettings(GroupName = "Public")]
[Tags("Public - Contact")]
public class PublicContactController : ControllerBase
{
    private readonly IContactService _contactService;

    public PublicContactController(IContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("Contact")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Send([FromBody] CreateContactMessageDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.FailureResponse("Invalid data"));

        if (Infrastructure.Services.Contact.ContactService.ContainsSpamKeywords(dto.Message))
            return BadRequest(ApiResponse<object>.FailureResponse("Message contains spam content"));

        await _contactService.CreateAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Message sent successfully"));
    }
}
