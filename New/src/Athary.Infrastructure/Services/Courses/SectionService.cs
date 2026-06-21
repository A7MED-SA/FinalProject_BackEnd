using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Athary.Domain.Entities;
using Athary.Domain.Interfaces;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.Courses;

public sealed class SectionService : ISectionService
{
    private readonly ApplicationDbContext _context;
    private readonly IRepository<Section> _sectionRepo;
    private readonly IRepository<SectionItem> _sectionItemRepo;
    private readonly IUnitOfWork _unitOfWork;

    public SectionService(
        ApplicationDbContext context,
        IRepository<Section> sectionRepo,
        IRepository<SectionItem> sectionItemRepo,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _sectionRepo = sectionRepo;
        _sectionItemRepo = sectionItemRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<SectionDto> CreateSectionAsync(Guid courseId, Guid instructorId, CreateSectionDto dto, CancellationToken cancellationToken = default)
    {
        await VerifyCourseOwnershipAsync(courseId, instructorId, cancellationToken);

        var currentMaxPosition = await _context.Sections
            .Where(s => s.CourseId == courseId)
            .MaxAsync(s => (int?)s.Position, cancellationToken) ?? 0;

        var section = new Section
        {
            CourseId = courseId,
            Title = dto.Title,
            Description = dto.Description,
            Position = currentMaxPosition + 1
        };

        await _sectionRepo.AddAsync(section, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(section);
    }

    public async Task<SectionDto> UpdateSectionAsync(Guid sectionId, Guid instructorId, UpdateSectionDto dto, CancellationToken cancellationToken = default)
    {
        var section = await GetSectionWithValidationAsync(sectionId, instructorId, cancellationToken);

        section.Title = dto.Title;
        section.Description = dto.Description;
        section.IsLocked = dto.IsLocked;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(section);
    }

    public async Task<SectionDto> GetSectionByIdAsync(Guid sectionId, CancellationToken cancellationToken = default)
    {
        var section = await _context.Sections
            .Include(s => s.SectionItems.OrderBy(si => si.Position))
            .FirstOrDefaultAsync(s => s.Id == sectionId, cancellationToken)
            ?? throw new KeyNotFoundException("Section not found.");

        return MapToDto(section);
    }

    public async Task DeleteSectionAsync(Guid sectionId, Guid instructorId, CancellationToken cancellationToken = default)
    {
        var section = await GetSectionWithValidationAsync(sectionId, instructorId, cancellationToken);

        await _sectionRepo.DeleteAsync(section, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<SectionDto>> GetSectionsForCourseAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        var sections = await _context.Sections
            .Include(s => s.SectionItems.OrderBy(si => si.Position))
            .Where(s => s.CourseId == courseId)
            .OrderBy(s => s.Position)
            .ToListAsync(cancellationToken);

        return sections.Select(MapToDto).ToList();
    }

    public async Task ReorderSectionsAsync(Guid courseId, Guid instructorId, ReorderRequestDto dto, CancellationToken cancellationToken = default)
    {
        await VerifyCourseOwnershipAsync(courseId, instructorId, cancellationToken);

        var validSectionIds = await _context.Sections
            .Where(s => s.CourseId == courseId)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        foreach (var item in dto.Items)
        {
            if (validSectionIds.Contains(item.Id))
            {
                await _context.Sections
                    .Where(s => s.Id == item.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.Position, item.Position), cancellationToken);
            }
        }
    }

    public async Task<SectionItemDto> AddItemToSectionAsync(Guid sectionId, Guid instructorId, CreateSectionItemDto dto, CancellationToken cancellationToken = default)
    {
        var section = await GetSectionWithValidationAsync(sectionId, instructorId, cancellationToken);

        var currentMaxPosition = await _context.SectionItems
            .Where(si => si.SectionId == sectionId)
            .MaxAsync(si => (int?)si.Position, cancellationToken) ?? 0;

        var item = new SectionItem
        {
            SectionId = sectionId,
            ItemType = dto.ItemType,
            ItemId = dto.ItemId,
            Position = currentMaxPosition + 1,
            IsPreviewAllowed = dto.IsPreviewAllowed,
            IsMandatory = dto.IsMandatory
        };

        await _sectionItemRepo.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToItemDto(item);
    }

    public async Task<SectionItemDto> UpdateSectionItemAsync(Guid itemId, Guid instructorId, UpdateSectionItemDto dto, CancellationToken cancellationToken = default)
    {
        var item = await _context.SectionItems
            .Include(si => si.Section)
            .FirstOrDefaultAsync(si => si.Id == itemId, cancellationToken)
            ?? throw new KeyNotFoundException("Section item not found.");

        await VerifyCourseOwnershipAsync(item.Section.CourseId, instructorId, cancellationToken);

        item.IsPreviewAllowed = dto.IsPreviewAllowed;
        item.IsMandatory = dto.IsMandatory;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToItemDto(item);
    }

    public async Task DeleteSectionItemAsync(Guid itemId, Guid instructorId, CancellationToken cancellationToken = default)
    {
        var item = await _context.SectionItems
            .Include(si => si.Section)
            .FirstOrDefaultAsync(si => si.Id == itemId, cancellationToken)
            ?? throw new KeyNotFoundException("Section item not found.");

        await VerifyCourseOwnershipAsync(item.Section.CourseId, instructorId, cancellationToken);

        await _sectionItemRepo.DeleteAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ReorderSectionItemsAsync(Guid sectionId, Guid instructorId, ReorderRequestDto dto, CancellationToken cancellationToken = default)
    {
        await GetSectionWithValidationAsync(sectionId, instructorId, cancellationToken);

        var validItemIds = await _context.SectionItems
            .Where(si => si.SectionId == sectionId)
            .Select(si => si.Id)
            .ToListAsync(cancellationToken);

        foreach (var item in dto.Items)
        {
            if (validItemIds.Contains(item.Id))
            {
                await _context.SectionItems
                    .Where(si => si.Id == item.Id)
                    .ExecuteUpdateAsync(si => si.SetProperty(x => x.Position, item.Position), cancellationToken);
            }
        }
    }

    private async Task VerifyCourseOwnershipAsync(Guid courseId, Guid instructorId, CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken)
            ?? throw new KeyNotFoundException("Course not found.");

        if (course.CreatedBy != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to modify this course.");
    }

    private async Task<Section> GetSectionWithValidationAsync(Guid sectionId, Guid instructorId, CancellationToken cancellationToken = default)
    {
        var section = await _context.Sections
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.Id == sectionId, cancellationToken)
            ?? throw new KeyNotFoundException("Section not found.");

        if (section.Course.CreatedBy != instructorId)
            throw new UnauthorizedAccessException("You do not have permission to modify this section.");

        return section;
    }

    private static SectionDto MapToDto(Section section)
    {
        return new SectionDto
        {
            Id = section.Id,
            CourseId = section.CourseId,
            Title = section.Title,
            Description = section.Description,
            Position = section.Position,
            IsLocked = section.IsLocked,
            Items = section.SectionItems?.Select(MapToItemDto).ToList() ?? new()
        };
    }

    private static SectionItemDto MapToItemDto(SectionItem item)
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
