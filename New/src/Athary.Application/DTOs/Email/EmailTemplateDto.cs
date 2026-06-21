namespace Athary.Application.DTOs.Email;

public sealed record EmailTemplateDto
{
    public string TemplateName { get; init; } = string.Empty;
    public Dictionary<string, object> Parameters { get; init; } = new();
}
