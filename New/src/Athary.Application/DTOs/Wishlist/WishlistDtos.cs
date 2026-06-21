namespace Athary.Application.DTOs.Wishlist;

public class WishlistItemDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseImageUrl { get; set; }
    public string? InstructorName { get; set; }
    public decimal Price { get; set; }
    public DateTime AddedAt { get; set; }
}

public class WishlistResponseDto
{
    public List<WishlistItemDto> Items { get; set; } = new();
    public int Count { get; set; }
}
