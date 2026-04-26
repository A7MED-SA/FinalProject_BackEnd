using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend_project.DTOs.Category;

namespace backend_project.Services.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryResponseDto>> GetAllCategoriesAsync();
    Task<CategoryResponseDto> GetCategoryByIdAsync(Guid id);
    Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto dto);
    Task<CategoryResponseDto> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto);
    Task DeleteCategoryAsync(Guid id);
    Task<CategoryResponseDto> SetCategoryImageAsync(Guid categoryId, Guid fileId, Guid userId);
}
