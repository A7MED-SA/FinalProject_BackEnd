using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using backend_project.Data;
using backend_project.DTOs.Communication;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class SystemSettingService : ISystemSettingService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly IActivityLogService _activityLogService;
    private const string CacheKey = "SystemSettings";

    public SystemSettingService(
        ApplicationDbContext context,
        IMemoryCache cache,
        IActivityLogService activityLogService)
    {
        _context = context;
        _cache = cache;
        _activityLogService = activityLogService;
    }

    public async Task<IEnumerable<SettingResponse>> GetAllAsync(string? group = null)
    {
        var settings = await GetCachedSettingsAsync();
        IEnumerable<SystemSetting> filtered = settings;
        if (!string.IsNullOrEmpty(group))
        {
            filtered = settings.Where(s =>
                s.SettingGroup.Equals(group, StringComparison.OrdinalIgnoreCase));
        }
        return filtered.Select(MapToResponse);
    }

    public async Task<SettingResponse> GetByKeyAsync(string key)
    {
        var settings = await GetCachedSettingsAsync();
        var setting = settings.FirstOrDefault(s =>
            s.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException($"Setting '{key}' not found.");
        return MapToResponse(setting);
    }

    public async Task<SettingResponse> UpdateAsync(string key, UpdateSettingRequest request, Guid userId)
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == key)
            ?? throw new KeyNotFoundException($"Setting '{key}' not found.");

        var oldValue = setting.Value;

        ValidateValue(request.Value, setting.DataType);

        setting.Value = request.Value;
        setting.UpdatedAt = DateTime.UtcNow;
        setting.UpdatedBy = userId;

        await _context.SaveChangesAsync();
        InvalidateCache();

        await _activityLogService.LogActivityAsync(
            userId, "SettingUpdated",
            $"{{ \"key\": \"{key}\", \"oldValue\": \"{oldValue}\", \"newValue\": \"{request.Value}\" }}",
            null);

        return MapToResponse(setting);
    }

    public async Task<SettingResponse> CreateAsync(CreateSettingRequest request, Guid userId)
    {
        var exists = await _context.SystemSettings.AnyAsync(s => s.Key == request.Key);
        if (exists)
            throw new InvalidOperationException($"Setting '{request.Key}' already exists.");

        if (!Enum.TryParse<SettingDataType>(request.DataType, out var dataType))
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

        _context.SystemSettings.Add(setting);
        await _context.SaveChangesAsync();
        InvalidateCache();

        return MapToResponse(setting);
    }

    public async Task DeleteAsync(string key)
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == key)
            ?? throw new KeyNotFoundException($"Setting '{key}' not found.");

        _context.SystemSettings.Remove(setting);
        await _context.SaveChangesAsync();
        InvalidateCache();
    }

    private async Task<List<SystemSetting>> GetCachedSettingsAsync()
    {
        if (_cache.TryGetValue(CacheKey, out List<SystemSetting>? cached))
            return cached!;

        var settings = await _context.SystemSettings.ToListAsync();
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
                try
                {
                    System.Text.Json.JsonDocument.Parse(value);
                }
                catch
                {
                    throw new InvalidOperationException("Value must be valid JSON.");
                }
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
            Description = s.Description ?? string.Empty,
            UpdatedAt = s.UpdatedAt,
            UpdatedBy = s.UpdatedBy
        };
    }
}
