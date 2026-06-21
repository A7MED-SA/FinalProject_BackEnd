using Athary.Application.DTOs.Communication;

namespace Athary.Application.Interfaces.Communication;

public interface IAnnouncementService
{
    Task<AnnouncementResponse> CreateAsync(Guid userId, CreateAnnouncementRequest request, CancellationToken cancellationToken = default);
    Task<AnnouncementResponse> UpdateAsync(Guid id, Guid userId, UpdateAnnouncementRequest request, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<AnnouncementListResponse> GetFeedAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
}
