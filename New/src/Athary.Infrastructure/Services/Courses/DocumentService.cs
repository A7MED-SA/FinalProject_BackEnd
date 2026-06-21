using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Athary.Application.Interfaces.Media;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Domain.Interfaces;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Athary.Infrastructure.Services.Courses;

public sealed class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IRepository<Document> _documentRepo;
    private readonly IRepository<SectionItem> _sectionItemRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IObjectStorage _objectStorage;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        ApplicationDbContext context,
        IRepository<Document> documentRepo,
        IRepository<SectionItem> sectionItemRepo,
        IUnitOfWork unitOfWork,
        IObjectStorage objectStorage,
        ILogger<DocumentService> logger)
    {
        _context = context;
        _documentRepo = documentRepo;
        _sectionItemRepo = sectionItemRepo;
        _unitOfWork = unitOfWork;
        _objectStorage = objectStorage;
        _logger = logger;
    }

    public async Task<DocumentResponseDto> GetDocumentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepo.FirstOrDefaultAsync(
            d => d.Id == id,
            include: q => q.Include(d => d.SectionItem!).ThenInclude(si => si.Section),
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException("Document not found.");

        return MapToDto(document);
    }

    public async Task<DocumentResponseDto> CreateDocumentAsync(Guid courseId, CreateDocumentDto createDto, CancellationToken cancellationToken = default)
    {
        var section = await _context.Sections
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == createDto.SectionId && s.CourseId == courseId, cancellationToken)
            ?? throw new KeyNotFoundException("Section not found or does not belong to this course.");

        var document = new Document
        {
            Title = createDto.Title,
            Description = createDto.Description,
            FileId = createDto.FileId,
            DownloadCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _documentRepo.AddAsync(document, cancellationToken);

        var position = await _context.SectionItems
            .CountAsync(si => si.SectionId == createDto.SectionId, cancellationToken) + 1;

        var sectionItem = new SectionItem
        {
            SectionId = createDto.SectionId,
            ItemType = SectionItemType.Document,
            ItemId = document.Id,
            Position = position,
            IsPreviewAllowed = true,
            IsMandatory = true
        };

        await _sectionItemRepo.AddAsync(sectionItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document {DocumentId} created in section {SectionId} (course {CourseId})", document.Id, createDto.SectionId, courseId);

        return MapToDto(document);
    }

    public async Task<DocumentResponseDto> UpdateDocumentAsync(Guid id, UpdateDocumentDto updateDto, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Document not found.");

        document.Title = updateDto.Title;
        document.Description = updateDto.Description;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document {DocumentId} updated", id);

        return MapToDto(document);
    }

    public async Task<bool> DeleteDocumentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepo.FirstOrDefaultAsync(
            d => d.Id == id,
            include: q => q.Include(d => d.SectionItem),
            cancellationToken: cancellationToken);

        if (document is null)
            return false;

        if (document.SectionItem is not null)
            await _sectionItemRepo.DeleteAsync(document.SectionItem, cancellationToken);

        await _documentRepo.DeleteAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document {DocumentId} deleted", id);

        return true;
    }

    private DocumentResponseDto MapToDto(Document document)
    {
        return new DocumentResponseDto
        {
            Id = document.Id,
            Title = document.Title,
            Description = document.Description,
            FileUrl = _objectStorage.GetPublicUrl("documents", document.FileId.ToString()),
            DownloadCount = document.DownloadCount,
            IsDownloadable = true,
            CreatedAt = document.CreatedAt
        };
    }
}
