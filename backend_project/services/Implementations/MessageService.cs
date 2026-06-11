using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using backend_project.Data;
using backend_project.DTOs.Communication;
using backend_project.Hubs;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<MessageHub> _hubContext;
    private const int MaxMessageLength = 5000;

    public MessageService(ApplicationDbContext context, IHubContext<MessageHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<MessageResponse> SendMessageAsync(Guid senderId, Guid receiverId, string content)
    {
        if (senderId == receiverId)
            throw new InvalidOperationException("Cannot send a message to yourself.");

        if (string.IsNullOrWhiteSpace(content) || content.Length > MaxMessageLength)
            throw new InvalidOperationException($"Message content must be between 1 and {MaxMessageLength} characters.");

        var sender = await _context.Users.FindAsync(senderId)
            ?? throw new KeyNotFoundException("Sender not found.");
        var receiver = await _context.Users.FindAsync(receiverId)
            ?? throw new KeyNotFoundException("Receiver not found.");

        bool hasExistingConversation = await _context.Messages
            .AnyAsync(m =>
                (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                (m.SenderId == receiverId && m.ReceiverId == senderId));

        if (!hasExistingConversation)
        {
            bool hasRelationship = await ValidateRelationshipAsync(senderId, receiverId);
            if (!hasRelationship)
                throw new InvalidOperationException("You do not have a valid relationship with the recipient to send a message.");
        }

        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        var response = MapToResponse(message);

        await _hubContext.Clients.User(receiverId.ToString()).SendAsync("NewMessage", response);
        await _hubContext.Clients.User(receiverId.ToString()).SendAsync("UnreadCountUpdate", new { unreadCount = await _context.Messages.CountAsync(m => m.ReceiverId == receiverId && !m.IsRead && !m.IsDeleted) });
        await _hubContext.Clients.User(senderId.ToString()).SendAsync("MessageSent", response);

        return response;
    }

    public async Task<ConversationListResponse> GetConversationsAsync(Guid userId, int page, int pageSize)
    {
        var messages = await _context.Messages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();

        var conversations = messages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g =>
            {
                var otherUser = g.First().SenderId == userId
                    ? g.First().Receiver
                    : g.First().Sender;
                var lastMessage = g.OrderByDescending(m => m.SentAt).First();
                return new ConversationResponse
                {
                    OtherUserId = otherUser.Id,
                    OtherUserName = $"{otherUser.FirstName} {otherUser.LastName}",
                    LastMessage = lastMessage.Content,
                    LastMessageAt = lastMessage.SentAt,
                    UnreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead)
                };
            })
            .OrderByDescending(c => c.LastMessageAt)
            .ToList();

        var totalCount = conversations.Count;
        var items = conversations.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new ConversationListResponse
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ConversationMessagesResponse> GetConversationMessagesAsync(Guid userId, Guid otherUserId, int page, int pageSize)
    {
        var query = _context.Messages
            .Where(m =>
                (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                (m.SenderId == otherUserId && m.ReceiverId == userId))
            .OrderBy(m => m.SentAt);

        var totalCount = await query.CountAsync();
        var messages = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new ConversationMessagesResponse
        {
            Items = messages.Select(m => new MessageResponse
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                Content = m.Content,
                SentAt = m.SentAt,
                IsRead = m.IsRead,
                ReadAt = m.ReadAt,
                IsDeletedForSender = m.IsDeleted && m.SenderId == userId
            }),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<UnreadCountResponse> GetUnreadCountAsync(Guid userId)
    {
        var count = await _context.Messages
            .CountAsync(m => m.ReceiverId == userId && !m.IsRead && !m.IsDeleted);
        return new UnreadCountResponse { UnreadCount = count };
    }

    public async Task MarkAsReadAsync(Guid messageId, Guid userId)
    {
        var message = await _context.Messages.FindAsync(messageId)
            ?? throw new KeyNotFoundException("Message not found.");

        if (message.ReceiverId != userId)
            throw new InvalidOperationException("You can only mark your own messages as read.");

        if (!message.IsRead)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(message.SenderId.ToString()).SendAsync("MessageRead", new { messageId = message.Id, readAt = message.ReadAt });
            await _hubContext.Clients.User(userId.ToString()).SendAsync("UnreadCountUpdate", new { unreadCount = await _context.Messages.CountAsync(m => m.ReceiverId == userId && !m.IsRead && !m.IsDeleted) });
        }
    }

    public async Task DeleteMessageAsync(Guid messageId, Guid userId)
    {
        var message = await _context.Messages.FindAsync(messageId)
            ?? throw new KeyNotFoundException("Message not found.");

        if (message.SenderId != userId)
            throw new InvalidOperationException("You can only delete your own messages.");

        message.IsDeleted = true;
        await _context.SaveChangesAsync();

        await _hubContext.Clients.User(message.ReceiverId.ToString()).SendAsync("MessageDeleted", new { messageId = message.Id });
        await _hubContext.Clients.User(message.ReceiverId.ToString()).SendAsync("UnreadCountUpdate", new { unreadCount = await _context.Messages.CountAsync(m => m.ReceiverId == message.ReceiverId && !m.IsRead && !m.IsDeleted) });
    }

    private async Task<bool> ValidateRelationshipAsync(Guid senderId, Guid receiverId)
    {
        var sender = await _context.Users.FindAsync(senderId);
        var receiver = await _context.Users.FindAsync(receiverId);

        if (sender == null || receiver == null)
            return false;

        var adminRoleId = await _context.Roles
            .Where(r => r.Name == "Admin")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        bool isSenderAdmin = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == senderId && ur.RoleId == adminRoleId);

        if (isSenderAdmin)
            return true;

        var instructorRoleId = await _context.Roles
            .Where(r => r.Name == "Instructor")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        var studentRoleId = await _context.Roles
            .Where(r => r.Name == "Student")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        bool isSenderInstructor = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == senderId && ur.RoleId == instructorRoleId);
        bool isReceiverStudent = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == receiverId && ur.RoleId == studentRoleId);
        bool isSenderStudent = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == senderId && ur.RoleId == studentRoleId);
        bool isReceiverInstructor = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == receiverId && ur.RoleId == instructorRoleId);

        if (isSenderInstructor && isReceiverStudent)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.UserId == receiverId && e.Course.CreatedBy == senderId);
        }

        if (isSenderStudent && isReceiverInstructor)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.UserId == senderId && e.Course.CreatedBy == receiverId);
        }

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
            ReadAt = message.ReadAt,
            IsDeletedForSender = false
        };
    }
}
