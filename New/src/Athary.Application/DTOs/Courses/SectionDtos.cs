using Athary.Domain.Enums;

namespace Athary.Application.DTOs.Courses;

public sealed record SectionDto
{
    public Guid Id { get; init; }
    public Guid CourseId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Position { get; init; }
    public bool IsLocked { get; init; }
    public List<SectionItemDto> Items { get; init; } = new();
}

public sealed record SectionItemDto
{
    public Guid Id { get; init; }
    public Guid SectionId { get; init; }
    public SectionItemType ItemType { get; init; }
    public Guid ItemId { get; init; }
    public int Position { get; init; }
    public bool IsPreviewAllowed { get; init; }
    public bool IsMandatory { get; init; }
}

public sealed record CreateSectionDto
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public sealed record UpdateSectionDto
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsLocked { get; init; }
}

public sealed record CreateSectionItemDto
{
    public SectionItemType ItemType { get; init; }
    public Guid ItemId { get; init; }
    public bool IsPreviewAllowed { get; init; }
    public bool IsMandatory { get; init; } = true;
}

public sealed record UpdateSectionItemDto
{
    public bool IsPreviewAllowed { get; init; }
    public bool IsMandatory { get; init; }
}

public sealed record ReorderItemDto
{
    public Guid Id { get; init; }
    public int Position { get; init; }
}

public sealed record ReorderRequestDto
{
    public List<ReorderItemDto> Items { get; init; } = new();
}
