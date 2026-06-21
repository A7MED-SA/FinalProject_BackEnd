using Athary.Application.DTOs.Communication;
using Athary.Application.Interfaces.Communication;
using Athary.Domain.Entities;
using Athary.Domain.Interfaces;
using Athary.Infrastructure.Data;
using Athary.Infrastructure.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.Communication;

public sealed class MessageService : IMessageService
{
    private readonly IRepository<Message> _messageRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<MessageHub> _hubContext;
    private readonly RoleManager<Role> _roleManager;
    private const int MaxMessageLength = 5000;

    public MessageService(
        IRepository<Message> messageRepo,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context,
        IHubContext<MessageHub> hubContext,
        RoleManager<Role> roleManager)
    {
        _messageRepo = messageRepo;
        _unitOfWork = unitOfWork;
        _context = context;
        _hubContext = hubContext;
        _roleManager = roleManager;
    }

    public async Task<MessageResponse> SendMessageAsync(Guid senderId, SendMessageRequest request, CancellationToken cancellationToken = default)
    {
        if (senderId == request.ReceiverId)
            throw new InvalidOperationException("Cannot send a message to yourself.");

        if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Length > MaxMessageLength)
            throw new InvalidOperationException($"Message content must be between 1 and {MaxMessageLength} characters.");

        var hasExistingConversation = await _messageRepo.AnyAsync(m =>
            (m.SenderId == senderId && m.ReceiverId == request.ReceiverId) ||
            (m.SenderId == request.ReceiverId && m.ReceiverId == senderId), cancellationToken);

        if (!hasExistingConversation)
        {
            var canMessage = await ValidateRelationshipAsync(senderId, request.ReceiverId, cancellationToken);
            if (!canMessage)
                throw new InvalidOperationException("You do not have a valid relationship with the recipient to send a message.");
        }

        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = request.ReceiverId,
            Content = request.Content,
            SentAt = DateTime.UtcNow
        };

        await _messageRepo.AddAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = MapToResponse(message);

        var unreadCount = await _messageRepo.CountAsync(m => m.ReceiverId == request.ReceiverId && !m.IsRead, cancellationToken);

        await _hubContext.Clients.User(request.ReceiverId.ToString()).SendAsync("NewMessage", response, cancellationToken);
        await _hubContext.Clients.User(request.ReceiverId.ToString()).SendAsync("UnreadCountUpdate", new { unreadCount }, cancellationToken);
        await _hubContext.Clients.User(senderId.ToString()).SendAsync("MessageSent", response, cancellationToken);

        return response;
    }

    public async Task<ConversationListResponse> GetConversationsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var allMessages = await _messageRepo.FindAsync(m => m.SenderId == userId || m.ReceiverId == userId, cancellationToken);

        var grouped = allMessages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g =>
            {
                var lastMessage = g.OrderByDescending(m => m.SentAt).First();
                var otherUser = _context.Users.Find(g.Key);
                return new ConversationResponse
                {
                    OtherUserId = g.Key,
                    OtherUserName = otherUser != null ? $"{otherUser.FirstName} {otherUser.LastName}" : "Unknown",
                    LastMessage = lastMessage.Content,
                    LastMessageAt = lastMessage.SentAt,
                    UnreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead)
                };
            })
            .OrderByDescending(c => c.LastMessageAt)
            .ToList();

        var totalCount = grouped.Count;
        var items = grouped.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new ConversationListResponse
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ConversationMessagesResponse> GetConversationMessagesAsync(Guid userId, Guid otherUserId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = _context.Messages
            .Where(m =>
                (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                (m.SenderId == otherUserId && m.ReceiverId == userId))
            .OrderByDescending(m => m.SentAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var messages = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderBy(m => m.SentAt)
            .ToListAsync(cancellationToken);

        return new ConversationMessagesResponse
        {
            Items = messages.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<UnreadCountResponse> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var count = await _messageRepo.CountAsync(m => m.ReceiverId == userId && !m.IsRead, cancellationToken);
        return new UnreadCountResponse { UnreadCount = count };
    }

    public async Task MarkAsReadAsync(Guid messageId, Guid userId, CancellationToken cancellationToken = default)
    {
        var message = await _messageRepo.GetByIdAsync(messageId, cancellationToken);
        if (message == null)
            throw new KeyNotFoundException("Message not found.");

        if (message.ReceiverId != userId)
            throw new InvalidOperationException("You can only mark your own messages as read.");

        if (!message.IsRead)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;

            await _messageRepo.UpdateAsync(message, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var unreadCount = await _messageRepo.CountAsync(m => m.ReceiverId == userId && !m.IsRead, cancellationToken);

            await _hubContext.Clients.User(message.SenderId.ToString()).SendAsync("MessageRead", new { messageId = message.Id, readAt = message.ReadAt }, cancellationToken);
            await _hubContext.Clients.User(userId.ToString()).SendAsync("UnreadCountUpdate", new { unreadCount }, cancellationToken);
        }
    }

    public async Task DeleteMessageAsync(Guid messageId, Guid userId, CancellationToken cancellationToken = default)
    {
        var message = await _messageRepo.GetByIdAsync(messageId, cancellationToken);
        if (message == null)
            throw new KeyNotFoundException("Message not found.");

        if (message.SenderId != userId)
            throw new InvalidOperationException("You can only delete your own messages.");

        message.IsDeleted = true;
        await _messageRepo.UpdateAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _hubContext.Clients.User(message.ReceiverId.ToString()).SendAsync("MessageDeleted", new { messageId = message.Id }, cancellationToken);

        var unreadCount = await _messageRepo.CountAsync(m => m.ReceiverId == message.ReceiverId && !m.IsRead, cancellationToken);
        await _hubContext.Clients.User(message.ReceiverId.ToString()).SendAsync("UnreadCountUpdate", new { unreadCount }, cancellationToken);
    }

    private async Task<bool> ValidateRelationshipAsync(Guid senderId, Guid receiverId, CancellationToken cancellationToken)
    {
        var adminRole = await _roleManager.FindByNameAsync("Admin");
        if (adminRole != null)
        {
            var isSenderAdmin = await _context.UserRoles.AnyAsync(ur => ur.UserId == senderId && ur.RoleId == adminRole.Id, cancellationToken);
            if (isSenderAdmin)
                return true;
        }

        var instructorRole = await _roleManager.FindByNameAsync("Instructor");
        var studentRole = await _roleManager.FindByNameAsync("Student");

        var instructorRoleId = instructorRole?.Id;
        var studentRoleId = studentRole?.Id;

        bool isSenderInstructor = instructorRoleId != null && await _context.UserRoles.AnyAsync(ur => ur.UserId == senderId && ur.RoleId == instructorRoleId, cancellationToken);
        bool isReceiverStudent = studentRoleId != null && await _context.UserRoles.AnyAsync(ur => ur.UserId == receiverId && ur.RoleId == studentRoleId, cancellationToken);
        bool isSenderStudent = studentRoleId != null && await _context.UserRoles.AnyAsync(ur => ur.UserId == senderId && ur.RoleId == studentRoleId, cancellationToken);
        bool isReceiverInstructor = instructorRoleId != null && await _context.UserRoles.AnyAsync(ur => ur.UserId == receiverId && ur.RoleId == instructorRoleId, cancellationToken);

        if (isSenderInstructor && isReceiverStudent)
            return await _context.Enrollments.AnyAsync(e => e.UserId == receiverId && e.Course.CreatedBy == senderId, cancellationToken);

        if (isSenderStudent && isReceiverInstructor)
            return await _context.Enrollments.AnyAsync(e => e.UserId == senderId && e.Course.CreatedBy == receiverId, cancellationToken);

        return false;
    }

    private static MessageResponse MapToResponse(Message message)
    {
        return new MessageResponse
        {
            Id = message.Id,
            SenderId = message.SenderId,
            ReceiverId = message.ReceiverId,
            Content = message.Content,
            SentAt = message.SentAt,
            IsRead = message.IsRead,
            ReadAt = message.ReadAt
        };
    }
}
