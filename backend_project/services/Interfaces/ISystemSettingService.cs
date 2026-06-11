using backend_project.DTOs.Communication;

namespace backend_project.Services.Interfaces;

public interface ISystemSettingService
{
    Task<IEnumerable<SettingResponse>> GetAllAsync(string? group = null);
    Task<SettingResponse> GetByKeyAsync(string key);
    Task<SettingResponse> UpdateAsync(string key, UpdateSettingRequest request, Guid userId);
    Task<SettingResponse> CreateAsync(CreateSettingRequest request, Guid userId);
    Task DeleteAsync(string key);
}
