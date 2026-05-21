using System;
using System.Threading.Tasks;
using backend_project.DTOs.Document;

namespace backend_project.Services.Interfaces;

public interface IDocumentService
{
    Task<DocumentResponseDto> GetDocumentAsync(Guid id);
    Task<DocumentResponseDto> CreateDocumentAsync(CreateDocumentDto createDto);
    Task<DocumentResponseDto> UpdateDocumentAsync(Guid id, UpdateDocumentDto updateDto);
    Task<bool> DeleteDocumentAsync(Guid id); // Handles SectionItem removal
}
