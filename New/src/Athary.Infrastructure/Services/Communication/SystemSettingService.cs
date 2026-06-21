using System.Text.Json;
using Athary.Application.DTOs.Communication;
using Athary.Application.Interfaces.Authentication;
using Athary.Application.Interfaces.Communication;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Athary.Infrastructure.Services.Communication;

public sealed class SystemSettingService : ISystemSettingService
{
    private readonly IRepository<SystemSetting> _settingRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly IActivityLogService _activityLogService;
    private const string CacheKey = "SystemSettings";

    public SystemSettingService(
        IRepository<SystemSetting> settingRepo,
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        IActivityLogService activityLogService)
    {
        _settingRepo = settingRepo;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _activityLogService = activityLogService;
    }

    public async Task<List<SettingResponse>> GetAllAsync(string? group = null, CancellationToken cancellationToken = default)
    {
        var settings = await GetCachedSettingsAsync(cancellationToken);

        if (!string.IsNullOrEmpty(group))
            settings = settings.Where(s => s.SettingGroup.Equals(group, StringComparison.OrdinalIgnoreCase)).ToList();

        return settings.Select(MapToResponse).ToList();
    }

    public async Task<SettingResponse> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var settings = await GetCachedSettingsAsync(cancellationToken);
        var setting = settings.FirstOrDefault(s => s.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException($"Setting '{key}' not found.");

        return MapToResponse(setting);
    }

    public async Task<SettingResponse> CreateAsync(CreateSettingRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var exists = await _settingRepo.AnyAsync(s => s.Key == request.Key, cancellationToken);
        if (exists)
            throw new InvalidOperationException($"Setting '{request.Key}' already exists.");

        if (!Enum.TryParse<SettingDataType>(request.DataType, true, out var dataType))
            throw new InvalidOperationException($"Invalid data type '{request.DataType}'.");

        ValidateValue(request.Value, dataType);

        var setting = new SystemSetting
        {
            SettingGroup = request.Group,
            Key = request.Key,
            Value = request.Value,
            DataType = dataType,
            Description = request.Description,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = userId
        };

        await _settingRepo.AddAsync(setting, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        InvalidateCache();

        return MapToResponse(setting);
    }

    public async Task<SettingResponse> UpdateAsync(string key, UpdateSettingRequest request, Guid userId, CancellationToken cancellationToken = default)
    {

        var settings = await _settingRepo.FindAsync(s => s.Key == key, cancellationToken);
        var setting = settings.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Setting '{key}' not found.");

        var oldValue = setting.Value;
        ValidateValue(request.Value, setting.DataType);

        setting.Value = request.Value;
        setting.UpdatedAt = DateTime.UtcNow;
        setting.UpdatedBy = userId;

        await _settingRepo.UpdateAsync(setting, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        InvalidateCache();

        await _activityLogService.LogActivityAsync(
            userId, "SettingUpdated",
            $"Setting '{key}' updated: '{oldValue}' → '{request.Value}'",
            string.Empty,
            cancellationToken: cancellationToken);

        return MapToResponse(setting);
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var settings = await _settingRepo.FindAsync(s => s.Key == key, cancellationToken);
        var setting = settings.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Setting '{key}' not found.");

        await _settingRepo.DeleteAsync(setting, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        InvalidateCache();
    }

    private async Task<List<SystemSetting>> GetCachedSettingsAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheKey, out List<SystemSetting>? cached) && cached != null)
            return cached;

        var settings = (await _settingRepo.GetAllAsync(cancellationToken)).ToList();
        _cache.Set(CacheKey, settings, TimeSpan.FromSeconds(60));
        return settings;
    }

    private void InvalidateCache()
    {
        _cache.Remove(CacheKey);
    }

    private static void ValidateValue(string value, SettingDataType dataType)
    {
        switch (dataType)
        {
            case SettingDataType.Integer:
                if (!int.TryParse(value, out _))
                    throw new InvalidOperationException("Value must be a valid integer.");
                break;
            case SettingDataType.Boolean:
                if (!bool.TryParse(value, out _))
                    throw new InvalidOperationException("Value must be 'true' or 'false'.");
                break;
            case SettingDataType.Json:
                try { JsonDocument.Parse(value); }
                catch { throw new InvalidOperationException("Value must be valid JSON."); }
                break;
        }
    }

    private static SettingResponse MapToResponse(SystemSetting s)
    {
        return new SettingResponse
        {
            Id = s.Id,
            Group = s.SettingGroup,
            Key = s.Key,
            Value = s.Value ?? string.Empty,
            DataType = s.DataType.ToString(),
            Description = s.Description,
            IsPublic = s.IsPublic,
            UpdatedAt = s.UpdatedAt,
            UpdatedBy = s.UpdatedBy
        };
    }
}
