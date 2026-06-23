using Athary.Application.Common;
using Athary.Application.DTOs.Contact;
using Athary.Application.Interfaces.Contact;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/admin/contact")]
[Authorize(Roles = "Admin")]
[ApiExplorerSettings(GroupName = "Admin")]
[Tags("Admin - Contact")]
public class AdminContactController : ControllerBase
{
    private readonly IContactService _contactService;

    public AdminContactController(IContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ContactMessageDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isRead = null, CancellationToken cancellationToken = default)
    {
        var messages = await _contactService.GetAllAsync(isRead, cancellationToken);
        return Ok(ApiResponse<List<ContactMessageDto>>.SuccessResponse(messages));
    }

    [HttpPut("{id:guid}/mark-read")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        await _contactService.MarkAsReadAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Message marked as read"));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _contactService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
