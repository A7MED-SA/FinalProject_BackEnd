namespace Athary.Domain.Entities;

public sealed class CourseRequirement : BaseEntity
{
    public Guid CourseId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public Course Course { get; set; } = null!;
}
