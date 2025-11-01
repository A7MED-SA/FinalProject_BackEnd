using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_project.Models;

[Table("system_settings")]
public class SystemSetting : BaseEntity
{

    [Required]
    [Column("setting_group")]
    [MaxLength(50)]
    public string SettingGroup { get; set; } = "general";

    [Required]
    [Column("key")]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [Column("value")]
    public string? Value { get; set; }

    [Column("data_type")]
    [MaxLength(20)]
    public SettingDataType DataType { get; set; } = SettingDataType.String;

    [Column("description")]
    public string? Description { get; set; }

    [Column("is_public")]
    public bool IsPublic { get; set; } = false;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("updated_by")]
    public Guid? UpdatedBy { get; set; }

    // Navigation Properties
    [ForeignKey("UpdatedBy")]
    public virtual User? UpdatedByUser { get; set; }
}

public enum SettingDataType
{
    String,
    Integer,
    Boolean,
    Json
}
