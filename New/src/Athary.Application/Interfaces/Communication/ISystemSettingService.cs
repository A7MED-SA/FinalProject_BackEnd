using Athary.Application.DTOs.Communication;

namespace Athary.Application.Interfaces.Communication;

public interface ISystemSettingService
{
    Task<List<SettingResponse>> GetAllAsync(string? group = null, CancellationToken cancellationToken = default);
    Task<SettingResponse> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<SettingResponse> CreateAsync(CreateSettingRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<SettingResponse> UpdateAsync(string key, UpdateSettingRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}
