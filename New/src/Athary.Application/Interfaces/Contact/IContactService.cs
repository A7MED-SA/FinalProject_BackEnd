using Athary.Application.DTOs.Contact;

namespace Athary.Application.Interfaces.Contact;

public interface IContactService
{
    Task<ContactMessageDto> CreateAsync(CreateContactMessageDto dto, CancellationToken cancellationToken = default);
    Task<List<ContactMessageDto>> GetAllAsync(bool? isRead = null, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
