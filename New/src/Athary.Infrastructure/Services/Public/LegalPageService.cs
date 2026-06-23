using Athary.Application.DTOs.Public;
using Athary.Application.Interfaces.Public;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Athary.Infrastructure.Services.Public;

public class LegalPageService : ILegalPageService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LegalPageService> _logger;

    public LegalPageService(
        ApplicationDbContext context,
        ILogger<LegalPageService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<LegalPageDto?> GetByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        var page = await _context.LegalPages
            .Where(lp => lp.Type == type && lp.IsPublished)
            .Select(lp => new LegalPageDto
            {
                Id = lp.Id,
                Type = lp.Type,
                Title = lp.Title,
                Content = lp.Content,
                IsPublished = lp.IsPublished,
                Version = lp.Version,
                LastUpdatedAt = lp.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return page;
    }
}
