namespace Athary.Application.DTOs.Public;

public sealed record TestimonialDto
{
    public Guid Id { get; init; }
    public string Content { get; init; } = string.Empty;
    public int Rating { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string? UserAvatar { get; init; }
    public int DisplayOrder { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed record CreateTestimonialDto
{
    public string Content { get; init; } = string.Empty;
    public int Rating { get; init; }
}

public sealed record UpdateTestimonialDto
{
    public string? Content { get; init; }
    public int? Rating { get; init; }
    public bool? IsApproved { get; init; }
    public int? DisplayOrder { get; init; }
}
