namespace Athary.Application.DTOs.Public;

public sealed record LegalPageDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public bool IsPublished { get; init; }
    public string? Version { get; init; }
    public DateTime? LastUpdatedAt { get; init; }
}
