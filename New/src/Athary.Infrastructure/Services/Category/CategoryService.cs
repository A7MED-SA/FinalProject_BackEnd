using System.Text.RegularExpressions;
using Athary.Application.DTOs.Category;
using Athary.Application.Interfaces.Category;
using Athary.Application.Interfaces.Media;
using Athary.Domain.Enums;
using Athary.Infrastructure.Data;
using Athary.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Athary.Infrastructure.Services.Category;

public sealed class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly IObjectStorage _objectStorage;
    private readonly IOptions<MinioSettings> _minioSettings;

    public CategoryService(
        ApplicationDbContext context,
        IObjectStorage objectStorage,
        IOptions<MinioSettings> minioSettings)
    {
        _context = context;
        _objectStorage = objectStorage;
        _minioSettings = minioSettings;
    }

    public async Task<List<CategoryResponseDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .Include(c => c.CategoryImageFile)
            .OrderBy(c => c.Position)
            .ToListAsync(cancellationToken);

        var lookup = categories.ToDictionary(c => c.Id);
        var roots = categories.Where(c => c.ParentCategoryId == null).ToList();

        return roots.Select(c => BuildCategoryTree(c, lookup)).ToList();
    }

    public async Task<CategoryResponseDto> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Include(c => c.CategoryImageFile)
            .Include(c => c.SubCategories)
                .ThenInclude(sc => sc.CategoryImageFile)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category == null)
            throw new KeyNotFoundException("Category not found.");

        var lookup = new Dictionary<Guid, Athary.Domain.Entities.Category> { { category.Id, category } };
        foreach (var sub in category.SubCategories)
            lookup[sub.Id] = sub;

        return BuildCategoryTree(category, lookup);
    }

    public async Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.ParentId.HasValue)
        {
            var parentExists = await _context.Categories.AnyAsync(c => c.Id == dto.ParentId.Value, cancellationToken);
            if (!parentExists)
                throw new KeyNotFoundException("Parent category not found.");
        }

        var slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Name) : dto.Slug;

        var category = new Athary.Domain.Entities.Category
        {
            Name = dto.Name,
            Slug = slug,
            Description = dto.Description,
            ParentCategoryId = dto.ParentId,
            Position = dto.Position,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentId = category.ParentCategoryId,
            Slug = category.Slug,
            Position = category.Position
        };
    }

    public async Task<CategoryResponseDto> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .Include(c => c.CategoryImageFile)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category == null)
            throw new KeyNotFoundException("Category not found.");

        if (dto.ParentId.HasValue && dto.ParentId.Value == id)
            throw new InvalidOperationException("A category cannot be its own parent.");

        category.Name = dto.Name;
            category.Description = dto.Description;
        category.ParentCategoryId = dto.ParentId;
        category.Position = dto.Position;

        var slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Name) : dto.Slug;
        category.Slug = slug;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(category);
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .Include(c => c.SubCategories)
            .Include(c => c.Courses)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category == null)
            throw new KeyNotFoundException("Category not found.");

        if (category.SubCategories.Count != 0)
            throw new InvalidOperationException("Cannot delete category with subcategories. Remove subcategories first.");

        if (category.Courses.Any(c => c.Status == CourseStatus.Published))
            throw new InvalidOperationException("Cannot delete category with active courses.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<CategoryResponseDto> SetCategoryImageAsync(Guid categoryId, Guid fileId, Guid userId, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .Include(c => c.CategoryImageFile)
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

        if (category == null)
            throw new KeyNotFoundException("Category not found.");

        var file = await _context.Files
            .FirstOrDefaultAsync(f => f.Id == fileId && f.UploadedBy == userId, cancellationToken);

        if (file == null)
            throw new KeyNotFoundException("File not found or you do not own this file.");

        if (file.FileType != StoredFileType.Image)
            throw new InvalidOperationException("File must be an image.");

        if (file.Status != FileStatus.Ready)
            throw new InvalidOperationException("File is not ready.");

        if (category.CategoryImageFileId.HasValue)
        {
            var oldFile = await _context.Files.FindAsync(new object[] { category.CategoryImageFileId.Value }, cancellationToken);
            if (oldFile != null)
            {
                oldFile.Status = FileStatus.Deleted;
                oldFile.DeletedAt = DateTime.UtcNow;
            }
        }

        file.Visibility = FileVisibility.Public;
        category.CategoryImageFileId = fileId;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(category);
    }

    private static CategoryResponseDto BuildCategoryTree(
        Athary.Domain.Entities.Category category,
        Dictionary<Guid, Athary.Domain.Entities.Category> lookup)
    {
        var dto = MapToDto(category);

        var children = lookup.Values
            .Where(c => c.ParentCategoryId == category.Id)
            .OrderBy(c => c.Position)
            .ToList();

        dto.Children = children.Select(c => BuildCategoryTree(c, lookup)).ToList();
        return dto;
    }

    private static CategoryResponseDto MapToDto(Athary.Domain.Entities.Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentId = category.ParentCategoryId,
            ImageUrl = category.CategoryImageFile != null
                ? $"/api/files/{category.CategoryImageFile.Id}/download"
                : null,
            Slug = category.Slug,
            Position = category.Position
        };
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant().Trim();
        slug = Regex.Replace(slug, @"[^\w\s-]", "");
        slug = Regex.Replace(slug, @"[\s_]+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');
        return slug;
    }
}
