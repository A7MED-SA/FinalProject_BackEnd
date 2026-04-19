using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.Models;
using backend_project.DTOs.Section;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class SectionService : ISectionService
{
    private readonly ApplicationDbContext _context;

    public SectionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SectionDto> CreateSectionAsync(Guid courseId, Guid instructorId, CreateSectionDto dto)
    {
        await VerifyCourseOwnershipAsync(courseId, instructorId);

        var currentMaxPosition = await _context.Sections
            .Where(s => s.CourseId == courseId)
            .MaxAsync(s => (int?)s.Position) ?? 0;

        var section = new Section
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = dto.Title,
            Description = dto.Description,
            Position = currentMaxPosition + 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.Sections.Add(section);
        await _context.SaveChangesAsync();

        return MapToDto(section);
    }

    public async Task<SectionDto> UpdateSectionAsync(Guid sectionId, Guid instructorId, UpdateSectionDto dto)
    {
        var section = await GetSectionWithValidationAsync(sectionId, instructorId);

        section.Title = dto.Title;
        section.Description = dto.Description;
        section.IsLocked = dto.IsLocked;

        await _context.SaveChangesAsync();

        return MapToDto(section);
    }

    public async Task<SectionDto> GetSectionByIdAsync(Guid sectionId)
    {
        var section = await _context.Sections
            .Include(s => s.SectionItems)
            .FirstOrDefaultAsync(s => s.Id == sectionId);

        if (section == null)
            throw new KeyNotFoundException("Section not found.");

        return MapToDto(section);
    }

    public async Task DeleteSectionAsync(Guid sectionId, Guid instructorId)
    {
        var section = await GetSectionWithValidationAsync(sectionId, instructorId);
        
        _context.Sections.Remove(section);
        await _context.SaveChangesAsync();
    }

    public async Task<List<SectionDto>> GetSectionsForCourseAsync(Guid courseId)
    {
        var sections = await _context.Sections
            .Include(s => s.SectionItems)
            .Where(s => s.CourseId == courseId)
            .OrderBy(s => s.Position)
            .ToListAsync();

        return sections.Select(MapToDto).ToList();
    }

    public async Task<SectionItemDto> AddItemToSectionAsync(Guid sectionId, Guid instructorId, CreateSectionItemDto dto)
    {
        var section = await GetSectionWithValidationAsync(sectionId, instructorId);

        var currentMaxPosition = await _context.SectionItems
            .Where(si => si.SectionId == sectionId)
            .MaxAsync(si => (int?)si.Position) ?? 0;

        var item = new SectionItem
        {
            Id = Guid.NewGuid(),
            SectionId = sectionId,
            ItemType = dto.ItemType,
            ItemId = dto.ItemId,
            Position = currentMaxPosition + 1,
            IsPreviewAllowed = dto.IsPreviewAllowed,
            IsMandatory = dto.IsMandatory
        };

        _context.SectionItems.Add(item);
        await _context.SaveChangesAsync();

        return MapToItemDto(item);
    }

    public async Task<SectionItemDto> UpdateSectionItemAsync(Guid itemId, Guid instructorId, UpdateSectionItemDto dto)
    {
        var item = await _context.SectionItems
            .Include(si => si.Section)
            .FirstOrDefaultAsync(si => si.Id == itemId);

        if (item == null)
            throw new KeyNotFoundException("Section item not found.");

        await VerifyCourseOwnershipAsync(item.Section.CourseId, instructorId);

        item.IsPreviewAllowed = dto.IsPreviewAllowed;
        item.IsMandatory = dto.IsMandatory;

        await _context.SaveChangesAsync();

        return MapToItemDto(item);
    }

    public async Task DeleteSectionItemAsync(Guid itemId, Guid instructorId)
    {
        var item = await _context.SectionItems
            .Include(si => si.Section)
            .FirstOrDefaultAsync(si => si.Id == itemId);

        if (item == null)
            throw new KeyNotFoundException("Section item not found.");

        await VerifyCourseOwnershipAsync(item.Section.CourseId, instructorId);

        _context.SectionItems.Remove(item);
        await _context.SaveChangesAsync();
    }

    public async Task ReorderSectionsAsync(Guid courseId, Guid instructorId, ReorderRequestDto dto)
    {
        await VerifyCourseOwnershipAsync(courseId, instructorId);

        // Fetch valid section IDs for this course to ensure safety
        var validSectionIds = await _context.Sections
            .Where(s => s.CourseId == courseId)
            .Select(s => s.Id)
            .ToListAsync();

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in dto.Items)
            {
                if (validSectionIds.Contains(item.Id))
                {
                    await _context.Sections
                        .Where(s => s.Id == item.Id)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.Position, item.Position));
                }
            }
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ReorderSectionItemsAsync(Guid sectionId, Guid instructorId, ReorderRequestDto dto)
    {
        var section = await GetSectionWithValidationAsync(sectionId, instructorId);

        // Fetch valid section item IDs
        var validItemIds = await _context.SectionItems
            .Where(si => si.SectionId == sectionId)
            .Select(si => si.Id)
            .ToListAsync();

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in dto.Items)
            {
                if (validItemIds.Contains(item.Id))
                {
                    await _context.SectionItems
                        .Where(si => si.Id == item.Id)
                        .ExecuteUpdateAsync(si => si.SetProperty(x => x.Position, item.Position));
                }
            }
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task VerifyCourseOwnershipAsync(Guid courseId, Guid instructorId)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            throw new KeyNotFoundException("Course not found.");

        if (course.CreatedBy != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to modify this course.");
    }

    private async Task<Section> GetSectionWithValidationAsync(Guid sectionId, Guid instructorId)
    {
        var section = await _context.Sections
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.Id == sectionId);

        if (section == null)
            throw new KeyNotFoundException("Section not found.");

        if (section.Course.CreatedBy != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to modify this section.");

        return section;
    }

    private SectionDto MapToDto(Section section)
    {
        return new SectionDto
        {
            Id = section.Id,
            CourseId = section.CourseId,
            Title = section.Title,
            Description = section.Description,
            Position = section.Position,
            IsLocked = section.IsLocked,
            Items = section.SectionItems?.OrderBy(si => si.Position).Select(MapToItemDto).ToList() ?? new List<SectionItemDto>()
        };
    }

    private SectionItemDto MapToItemDto(SectionItem item)
    {
        return new SectionItemDto
        {
            Id = item.Id,
            SectionId = item.SectionId,
            ItemType = item.ItemType,
            ItemId = item.ItemId,
            Position = item.Position,
            IsPreviewAllowed = item.IsPreviewAllowed,
            IsMandatory = item.IsMandatory
        };
    }
}
