using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class SystemSetting : BaseEntity
{
    public string SettingGroup { get; set; } = "general";
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public SettingDataType DataType { get; set; } = SettingDataType.String;
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    public User? UpdatedByUser { get; set; }
}
