namespace Athary.Application.DTOs.Category;

public class CategoryResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public string? ImageUrl { get; set; }
    public string? Slug { get; set; }
    public int Position { get; set; }
    public List<CategoryResponseDto> Children { get; set; } = new();
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Slug { get; set; }
    public Guid? ParentId { get; set; }
    public int Position { get; set; }
}

public class UpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Slug { get; set; }
    public Guid? ParentId { get; set; }
    public int Position { get; set; }
}

public class SetCategoryImageRequest
{
    public Guid FileId { get; set; }
}
