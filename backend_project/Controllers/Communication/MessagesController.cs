using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using backend_project.DTOs;
using backend_project.DTOs.Communication;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers.Communication;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("Messaging")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            var result = await _messageService.SendMessageAsync(GetUserId(), request.ReceiverId, request.Content);
            return CreatedAtAction(nameof(GetConversationMessages), new { otherUserId = request.ReceiverId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _messageService.GetConversationsAsync(GetUserId(), page, pageSize);
        return Ok(ApiResponse<ConversationListResponse>.SuccessResponse(result));
    }

    [HttpGet("conversations/{otherUserId}")]
    public async Task<IActionResult> GetConversationMessages(Guid otherUserId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = await _messageService.GetConversationMessagesAsync(GetUserId(), otherUserId, page, pageSize);
        return Ok(ApiResponse<ConversationMessagesResponse>.SuccessResponse(result));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var result = await _messageService.GetUnreadCountAsync(GetUserId());
        return Ok(ApiResponse<UnreadCountResponse>.SuccessResponse(result));
    }

    [HttpPatch("{messageId}/read")]
    public async Task<IActionResult> MarkAsRead(Guid messageId)
    {
        try
        {
            await _messageService.MarkAsReadAsync(messageId, GetUserId());
            return Ok(ApiResponse<object>.SuccessResponse(new { }));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> DeleteMessage(Guid messageId)
    {
        try
        {
            await _messageService.DeleteMessageAsync(messageId, GetUserId());
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }
}
