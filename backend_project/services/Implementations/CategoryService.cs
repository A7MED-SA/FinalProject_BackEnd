using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.Models;
using backend_project.DTOs.Category;
using backend_project.Services.Interfaces;
using backend_project.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace backend_project.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly MinioSettings _minioSettings;
    private readonly IObjectStorage _objectStorage;

        public CategoryService(
        ApplicationDbContext context,
        IObjectStorage objectStorage,
        IOptions<MinioSettings> minioSettings
        )
    {
        _context = context;
        _objectStorage = objectStorage;
        _minioSettings = minioSettings.Value;
    }

    public async Task<List<CategoryResponseDto>> GetAllCategoriesAsync()
    {
        var categories = await _context.Categories
            .Include(c => c.CategoryImageFile)
            .OrderBy(c => c.Position)
            .ToListAsync();

        return BuildCategoryTree(categories);
    }

    public async Task<CategoryResponseDto> GetCategoryByIdAsync(Guid id)
    {
        var category = await _context.Categories
            .Include(c => c.CategoryImageFile)
            .FirstOrDefaultAsync(c => c.Id == id);
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
            Slug = !string.IsNullOrWhiteSpace(dto.Slug)
            ? dto.Slug
            : GenerateSlug(dto.Name),
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
    var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
    if (category == null)
        throw new KeyNotFoundException("Category not found");

    // 1. تحديث الاسم (Name)
    if (!string.IsNullOrWhiteSpace(dto.Name))
    {
        category.Name = dto.Name;
        
        // إذا تم تغيير الاسم ولم يتم إرسال Slug جديد، نقوم بإنشاء Slug جديد بناءً على الاسم
        if (string.IsNullOrWhiteSpace(dto.Slug))
        {
            category.Slug = GenerateSlug(dto.Name);
        }
    }

    // 2. تحديث الـ Slug (في حال تم إرساله صراحةً بغض النظر عن الاسم)
    if (!string.IsNullOrWhiteSpace(dto.Slug))
    {
        category.Slug = dto.Slug;
    }

    // 3. تحديث الوصف (تأكد أنك تقبل القيمة null إذا كان مسموحاً مسح الوصف)
    if (!string.IsNullOrWhiteSpace(dto.Description)) 
    {
        category.Description = dto.Description;
    }

    // 4. تحديث الـ ParentId (يجب أن يكون Guid? في الـ DTO)
    if (dto.ParentId.HasValue)
    {
        category.ParentCategoryId = dto.ParentId; 
        // أو dto.ParentId.Value حسب نوعها في قاعدة البيانات
    }

    // 5. تحديث الترتيب (يجب أن يكون int? في الـ DTO)
    if (dto.Position != null)
    {
        category.Position = dto.Position;
    }

    // لن يقوم Entity Framework بعمل Update إلا للحقول التي تغيرت قيمتها فعلياً
    await _context.SaveChangesAsync();

    return MapToDto(category);
}

    public async Task DeleteCategoryAsync(Guid id)
    {
        var category = await _context.Categories
            .Include(c => c.Courses)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            throw new KeyNotFoundException("Category not found");

        bool hasSubcategories = await _context.Categories
            .AnyAsync(c => c.ParentCategoryId == id && c.DeletedAt == null);

        if (hasSubcategories)
            throw new InvalidOperationException("Cannot delete category with subcategories. Remove or reassign subcategories first.");

        if (category.Courses != null && category.Courses.Any(c => c.DeletedAt == null))
            throw new InvalidOperationException("Cannot delete category with active courses. Reassign courses to another category first.");

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
    public async Task<CategoryResponseDto> SetCategoryImageAsync(Guid categoryId, Guid fileId,Guid userId)
    {
        // 1️⃣ جلب الفئة مع التأكد إنها مش محذوفة
        var category = await _context.Categories
            .Include(c => c.CategoryImageFile)
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.DeletedAt == null)
            ?? throw new KeyNotFoundException("Category not found");

        // 2️⃣ التحقق من صحة الملف
        var file = await _context.Files.FirstOrDefaultAsync(f =>
            f.Id == fileId &&
            f.FileType == StoredFileType.Image &&
            f.Status == FileStatus.Ready &&
            f.DeletedAt == null)
            ?? throw new InvalidOperationException("Invalid image file");

        // 3️⃣ 🔐 صلاحيات: التأكد إن الملف مرفوع بواسطة مستخدم له صلاحية (مثلاً أدمن أو صاحب الفئة)
        // ملاحظة: الفئات مش بالضرورة ليها "Owner"، فعدل الشرط حسب نظام الصلاحيات عندك
        if (file.UploadedBy != userId) // ⚠️ عدل هذا الشرط حسب الـ Authorization عندك
            throw new UnauthorizedAccessException("You are not authorized to use this file");

        // 4️⃣ Soft Delete للصورة القديمة لو موجودة ومختلفة
        if (category.CategoryImageFileId.HasValue && 
            category.CategoryImageFileId != file.Id)
        {
            var oldFile = await _context.Files
                .FirstOrDefaultAsync(f => 
                    f.Id == category.CategoryImageFileId && 
                    f.DeletedAt == null);

            if (oldFile != null)
            {
                oldFile.DeletedAt = DateTime.UtcNow;
                oldFile.Status = FileStatus.Deleted;
            }
        }

        // 5️⃣ تعيين الصورة الجديدة
        file.Visibility = FileVisibility.Public;
        
        category.CategoryImageFileId = file.Id;
        // category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(category);
    }
    private string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        return name.Trim()
            .ToLower()
            .Replace(' ', '-')
            .Replace('_', '-')
            .Replace('.', '-')
            .Replace(',', '-')
            .Replace('?', '-')
            .Replace('!', '-')
            .Replace('/', '-')
            .Replace('\\', '-')
            .Replace('#', '-')
            .Replace('&', '-')
            .Replace('%', '-')
            .Replace('@', '-')
            .Replace('+', '-')
            .Replace('=', '-')
            .Replace(':', '-')
            .Replace(';', '-')
            .Replace('\'', '-')
            .Replace('"', '-')
            .Replace('<', '-')
            .Replace('>', '-')
            .Replace('|', '-')
            .Replace('*', '-')
            .Replace('^', '-')
            .Replace('$', '-')
            .Replace('~', '-')
            .Replace('`', '-')
            .Replace('(', '-')
            .Replace(')', '-')
            .Replace('[', '-')
            .Replace(']', '-')
            .Replace('{', '-')
            .Replace('}', '-')
            .Replace("--", "-")
            .Trim('-');
    }

    private CategoryResponseDto MapToDto(Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentId = category.ParentCategoryId,
            ImageUrl = category.CategoryImageFile != null 
            ? _objectStorage.GetPublicUrl(
                    category.CategoryImageFile.Bucket,
                    category.CategoryImageFile.FilePath)
            : null,
            Slug = category.Slug,
            Position = category.Position,
            Children = new List<CategoryResponseDto>()
        };
    }
}
