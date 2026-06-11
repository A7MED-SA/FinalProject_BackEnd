namespace backend_project.DTOs.Communication;

public class CreateSettingRequest
{
    public string Group { get; set; } = "General";
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string DataType { get; set; } = "String";
    public string Description { get; set; } = string.Empty;
}

public class UpdateSettingRequest
{
    public string Value { get; set; } = string.Empty;
}

public class SettingResponse
{
    public Guid Id { get; set; }
    public string Group { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
