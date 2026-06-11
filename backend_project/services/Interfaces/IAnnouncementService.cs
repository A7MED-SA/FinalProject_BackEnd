using backend_project.DTOs.Communication;
using backend_project.Models;

namespace backend_project.Services.Interfaces;

public interface IAnnouncementService
{
    Task<AnnouncementResponse> CreateAsync(Guid userId, CreateAnnouncementRequest request);
    Task<AnnouncementResponse> UpdateAsync(Guid id, Guid userId, UpdateAnnouncementRequest request);
    Task DeactivateAsync(Guid id, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task<AnnouncementListResponse> GetFeedAsync(Guid userId, int page, int pageSize);
}
