using System;
using System.Collections.Generic;

namespace backend_project.DTOs.Category;

public class CategoryResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public string? ImageUrl { get; set; }
    public string? Slug { get; set; } 
    public int Position { get; set; }
    public List<CategoryResponseDto> Children { get; set; } = new();
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Slug { get; set; } // 👈 أضف هذا
    public Guid? ParentId { get; set; }
    public int Position { get; set; }
}
public class SetCategoryImageRequest
{
    public Guid FileId { get; set; }
}

public class UpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
        public string? Slug { get; set; } // 👈 أضف هذا
    public int Position { get; set; }
}
