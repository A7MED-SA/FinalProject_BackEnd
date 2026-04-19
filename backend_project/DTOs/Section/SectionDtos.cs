using System;
using System.Collections.Generic;
using backend_project.Models;

namespace backend_project.DTOs.Section;

public class SectionItemDto
{
    public Guid Id { get; set; }
    public Guid SectionId { get; set; }
    public SectionItemType ItemType { get; set; }
    public Guid ItemId { get; set; }
    public int Position { get; set; }
    public bool IsPreviewAllowed { get; set; }
    public bool IsMandatory { get; set; }
}

public class SectionDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Position { get; set; }
    public bool IsLocked { get; set; }
    public List<SectionItemDto> Items { get; set; } = new();
}

public class CreateSectionDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateSectionDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsLocked { get; set; }
}

public class CreateSectionItemDto
{
    public SectionItemType ItemType { get; set; }
    public Guid ItemId { get; set; }
    public bool IsPreviewAllowed { get; set; }
    public bool IsMandatory { get; set; } = true;
}

public class UpdateSectionItemDto
{
    public bool IsPreviewAllowed { get; set; }
    public bool IsMandatory { get; set; }
}
