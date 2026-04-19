using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.Models;
using backend_project.DTOs.Category;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryResponseDto>> GetAllCategoriesAsync()
    {
        var categories = await _context.Categories
            .OrderBy(c => c.Position)
            .ToListAsync();

        return BuildCategoryTree(categories);
    }

    public async Task<CategoryResponseDto> GetCategoryByIdAsync(Guid id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            throw new KeyNotFoundException("Category not found");

        return MapToDto(category);
    }

    public async Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            ParentCategoryId = dto.ParentId,
            Position = dto.Position,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<CategoryResponseDto> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            throw new KeyNotFoundException("Category not found");

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.ParentCategoryId = dto.ParentId;
        category.Position = dto.Position;

        await _context.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            throw new KeyNotFoundException("Category not found");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }

    private List<CategoryResponseDto> BuildCategoryTree(List<Category> allCategories)
    {
        var lookup = allCategories.ToDictionary(c => c.Id, MapToDto);
        var rootCategories = new List<CategoryResponseDto>();

        foreach (var category in allCategories)
        {
            var dto = lookup[category.Id];
            if (category.ParentCategoryId.HasValue && lookup.TryGetValue(category.ParentCategoryId.Value, out var parentDto))
            {
                parentDto.Children.Add(dto);
            }
            else
            {
                rootCategories.Add(dto);
            }
        }

        return rootCategories;
    }

    private CategoryResponseDto MapToDto(Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentId = category.ParentCategoryId,
            Position = category.Position,
            Children = new List<CategoryResponseDto>()
        };
    }
}
