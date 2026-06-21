using Athary.Application.DTOs.Communication;

namespace Athary.Application.Interfaces.Communication;

public interface IMessageService
{
    Task<MessageResponse> SendMessageAsync(Guid senderId, SendMessageRequest request, CancellationToken cancellationToken = default);
    Task<ConversationListResponse> GetConversationsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ConversationMessagesResponse> GetConversationMessagesAsync(Guid userId, Guid otherUserId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<UnreadCountResponse> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid messageId, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteMessageAsync(Guid messageId, Guid userId, CancellationToken cancellationToken = default);
}
