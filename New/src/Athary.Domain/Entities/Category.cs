namespace Athary.Domain.Entities;

public sealed class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Position { get; set; }
    public Guid? CategoryImageFileId { get; set; }
    public UploadedFile? CategoryImageFile { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public Category? ParentCategory { get; set; }
    public List<Category> SubCategories { get; set; } = new List<Category>();
    public List<Course> Courses { get; set; } = new List<Course>();
}
