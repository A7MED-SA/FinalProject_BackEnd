using Athary.Application.DTOs.Public;

namespace Athary.Application.Interfaces.Public;

public interface IPublicService
{
    Task<LandingDto> GetLandingDataAsync(CancellationToken cancellationToken = default);
}
