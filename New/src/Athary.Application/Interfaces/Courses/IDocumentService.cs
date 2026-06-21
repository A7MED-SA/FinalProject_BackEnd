using Athary.Application.DTOs.Courses;

namespace Athary.Application.Interfaces.Courses;

public interface IDocumentService
{
    Task<DocumentResponseDto> GetDocumentAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DocumentResponseDto> CreateDocumentAsync(Guid courseId, CreateDocumentDto createDto, CancellationToken cancellationToken = default);

    Task<DocumentResponseDto> UpdateDocumentAsync(Guid id, UpdateDocumentDto updateDto, CancellationToken cancellationToken = default);

    Task<bool> DeleteDocumentAsync(Guid id, CancellationToken cancellationToken = default);
}
