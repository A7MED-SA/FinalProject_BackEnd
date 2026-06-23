using Athary.Application.DTOs.Public;

namespace Athary.Application.Interfaces.Public;

public interface ILegalPageService
{
    Task<LegalPageDto?> GetByTypeAsync(string type, CancellationToken cancellationToken = default);
}
