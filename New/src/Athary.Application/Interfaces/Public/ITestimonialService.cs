using Athary.Application.DTOs.Public;

namespace Athary.Application.Interfaces.Public;

public interface ITestimonialService
{
    Task<List<TestimonialDto>> GetApprovedAsync(int page, int pageSize, int? minRating = null, CancellationToken cancellationToken = default);
    Task<TestimonialDto> CreateAsync(Guid userId, CreateTestimonialDto dto, CancellationToken cancellationToken = default);
    Task<TestimonialDto> UpdateAsync(Guid userId, CreateTestimonialDto dto, CancellationToken cancellationToken = default);
    Task<List<TestimonialDto>> GetAllAsync(bool? isApproved = null, CancellationToken cancellationToken = default);
    Task<TestimonialDto> UpdateAdminAsync(Guid id, UpdateTestimonialDto dto, CancellationToken cancellationToken = default);
    Task ApproveAsync(Guid id, CancellationToken cancellationToken = default);
    Task FlagAsync(Guid id, string reason, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task ReorderAsync(List<ReorderItemDto> items, CancellationToken cancellationToken = default);
}

public sealed record ReorderItemDto
{
    public Guid Id { get; init; }
    public int DisplayOrder { get; init; }
}
