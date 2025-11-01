namespace backend_project.DTOs.Email;

public class EmailTemplateDto
{
    public string TemplateName { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
}