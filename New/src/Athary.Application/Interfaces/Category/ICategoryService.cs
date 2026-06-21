using Athary.Application.DTOs.Category;

namespace Athary.Application.Interfaces.Category;

public interface ICategoryService
{
    Task<List<CategoryResponseDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<CategoryResponseDto> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);
    Task<CategoryResponseDto> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CategoryResponseDto> SetCategoryImageAsync(Guid categoryId, Guid fileId, Guid userId, CancellationToken cancellationToken = default);
}
