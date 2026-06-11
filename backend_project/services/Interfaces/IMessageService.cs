using backend_project.DTOs.Communication;
using backend_project.Models;

namespace backend_project.Services.Interfaces;

public interface IMessageService
{
    Task<MessageResponse> SendMessageAsync(Guid senderId, Guid receiverId, string content);
    Task<ConversationListResponse> GetConversationsAsync(Guid userId, int page, int pageSize);
    Task<ConversationMessagesResponse> GetConversationMessagesAsync(Guid userId, Guid otherUserId, int page, int pageSize);
    Task<UnreadCountResponse> GetUnreadCountAsync(Guid userId);
    Task MarkAsReadAsync(Guid messageId, Guid userId);
    Task DeleteMessageAsync(Guid messageId, Guid userId);
}
