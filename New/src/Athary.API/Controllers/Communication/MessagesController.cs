using Athary.Application.Common;
using Athary.Application.DTOs.Communication;
using Athary.Application.Interfaces.Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Athary.API.Controllers.Communication;

[ApiController]
[Route("api/messages")]
[Authorize]
[EnableRateLimiting("Messaging")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MessageResponse>>> SendMessage([FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _messageService.SendMessageAsync(userId, request, cancellationToken);
        return Ok(ApiResponse<MessageResponse>.SuccessResponse(result));
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<ApiResponse<ConversationListResponse>>> GetConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _messageService.GetConversationsAsync(userId, page, pageSize, cancellationToken);
        return Ok(ApiResponse<ConversationListResponse>.SuccessResponse(result));
    }

    [HttpGet("conversations/{otherUserId}")]
    public async Task<ActionResult<ApiResponse<ConversationMessagesResponse>>> GetConversationMessages(Guid otherUserId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _messageService.GetConversationMessagesAsync(userId, otherUserId, page, pageSize, cancellationToken);
        return Ok(ApiResponse<ConversationMessagesResponse>.SuccessResponse(result));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<UnreadCountResponse>>> GetUnreadCount(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _messageService.GetUnreadCountAsync(userId, cancellationToken);
        return Ok(ApiResponse<UnreadCountResponse>.SuccessResponse(result));
    }

    [HttpPatch("{messageId}/read")]
    public async Task<IActionResult> MarkAsRead(Guid messageId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _messageService.MarkAsReadAsync(messageId, userId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(new { }));
    }

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> DeleteMessage(Guid messageId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _messageService.DeleteMessageAsync(messageId, userId, cancellationToken);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (claim == null || !Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");
        return userId;
    }
}
