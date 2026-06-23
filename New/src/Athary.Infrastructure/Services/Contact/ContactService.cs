using Athary.Application.DTOs.Contact;
using Athary.Application.Interfaces.Contact;
using Athary.Domain.Entities;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Athary.Infrastructure.Services.Contact;

public class ContactService : IContactService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ContactService> _logger;

    private static readonly string[] SpamKeywords = new[]
    {
        "viagra", "casino", "lottery", "xxx", "free money",
        "click here", "buy now", "limited time offer"
    };

    public ContactService(
        ApplicationDbContext context,
        ILogger<ContactService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ContactMessageDto> CreateAsync(CreateContactMessageDto dto, CancellationToken cancellationToken = default)
    {
        var message = new ContactMessage
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Subject = dto.Subject,
            Message = dto.Message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.ContactMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Contact message created from {Email}", dto.Email);

        return MapToDto(message);
    }

    public async Task<List<ContactMessageDto>> GetAllAsync(bool? isRead = null, CancellationToken cancellationToken = default)
    {
        var query = _context.ContactMessages.AsQueryable();

        if (isRead.HasValue)
            query = query.Where(m => m.IsRead == isRead.Value);

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new ContactMessageDto
            {
                Id = m.Id,
                FullName = m.FullName,
                Email = m.Email,
                Phone = m.Phone,
                Subject = m.Subject,
                Message = m.Message,
                IsRead = m.IsRead,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var message = await _context.ContactMessages.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new KeyNotFoundException("Contact message not found");

        message.IsRead = true;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Contact message {Id} marked as read", id);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var message = await _context.ContactMessages.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new KeyNotFoundException("Contact message not found");

        _context.ContactMessages.Remove(message);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Contact message {Id} deleted", id);
    }

    public static bool ContainsSpamKeywords(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;

        var lowerContent = content.ToLower();
        return SpamKeywords.Any(keyword => lowerContent.Contains(keyword));
    }

    private static ContactMessageDto MapToDto(ContactMessage message)
    {
        return new ContactMessageDto
        {
            Id = message.Id,
            FullName = message.FullName,
            Email = message.Email,
            Phone = message.Phone,
            Subject = message.Subject,
            Message = message.Message,
            IsRead = message.IsRead,
            CreatedAt = message.CreatedAt
        };
    }
}
