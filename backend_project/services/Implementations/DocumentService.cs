using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.Document;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;

    public DocumentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DocumentResponseDto> GetDocumentAsync(Guid id)
    {
        var document = await _context.Documents
            .AsNoTracking()
            .Include(d => d.SectionItem)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
            throw new KeyNotFoundException("Document not found.");

        return MapToDto(document);
    }

    public async Task<DocumentResponseDto> CreateDocumentAsync(CreateDocumentDto createDto)
    {
        var section = await _context.Sections
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == createDto.SectionId);

        if (section == null)
            throw new KeyNotFoundException("Section not found.");

        var document = new Document
        {
            Id = Guid.NewGuid(),
            Title = createDto.Title,
            Description = createDto.Description,
            FileId = createDto.FileId,
            DownloadCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.Documents.Add(document);

        var sectionItem = new SectionItem
        {
            Id = Guid.NewGuid(),
            SectionId = createDto.SectionId,
            ItemType = SectionItemType.Document,
            ItemId = document.Id,
            Position = await _context.SectionItems
                .Where(si => si.SectionId == createDto.SectionId)
                .CountAsync() + 1,
            IsPreviewAllowed = true,
            IsMandatory = true
        };

        _context.SectionItems.Add(sectionItem);
        await _context.SaveChangesAsync();

        return await GetDocumentAsync(document.Id);
    }

    public async Task<DocumentResponseDto> UpdateDocumentAsync(Guid id, UpdateDocumentDto updateDto)
    {
        var document = await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
            throw new KeyNotFoundException("Document not found.");

        document.Title = updateDto.Title;
        document.Description = updateDto.Description;

        await _context.SaveChangesAsync();

        return MapToDto(document);
    }

    public async Task<bool> DeleteDocumentAsync(Guid id)
    {
        var document = await _context.Documents
            .Include(d => d.SectionItem)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
            return false;

        if (document.SectionItem != null)
            _context.SectionItems.Remove(document.SectionItem);

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();

        return true;
    }

    private static DocumentResponseDto MapToDto(Document document)
    {
        return new DocumentResponseDto
        {
            Id = document.Id,
            Title = document.Title,
            Description = document.Description,
            FileUrl = string.Empty,
            FileType = string.Empty,
            DownloadCount = document.DownloadCount,
            CreatedAt = document.CreatedAt
        };
    }
}
