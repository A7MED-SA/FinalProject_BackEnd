using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend_project.DTOs.Section;

namespace backend_project.Services.Interfaces;

public interface ISectionService
{
    // Sections
    Task<SectionDto> CreateSectionAsync(Guid courseId, Guid instructorId, CreateSectionDto dto);
    Task<SectionDto> UpdateSectionAsync(Guid sectionId, Guid instructorId, UpdateSectionDto dto);
    Task<SectionDto> GetSectionByIdAsync(Guid sectionId);
    Task DeleteSectionAsync(Guid sectionId, Guid instructorId);
    Task<List<SectionDto>> GetSectionsForCourseAsync(Guid courseId);
    Task ReorderSectionsAsync(Guid courseId, Guid instructorId, ReorderRequestDto dto);

    // Section Items
    Task<SectionItemDto> AddItemToSectionAsync(Guid sectionId, Guid instructorId, CreateSectionItemDto dto);
    Task<SectionItemDto> UpdateSectionItemAsync(Guid itemId, Guid instructorId, UpdateSectionItemDto dto);
    Task DeleteSectionItemAsync(Guid itemId, Guid instructorId);
    Task ReorderSectionItemsAsync(Guid sectionId, Guid instructorId, ReorderRequestDto dto);
}
