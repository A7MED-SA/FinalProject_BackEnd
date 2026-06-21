using Athary.Application.DTOs.LiveSession;

namespace Athary.Application.Interfaces.LiveSession;

public interface ILiveSessionService
{
    Task<LiveSessionResponseDto> CreateSessionAsync(CreateLiveSessionDto createDto, CancellationToken cancellationToken = default);
    Task<LiveSessionResponseDto> UpdateStatusAsync(Guid id, UpdateLiveSessionStatusDto statusDto, CancellationToken cancellationToken = default);
    Task<List<LiveSessionResponseDto>> GetSessionsForCourseAsync(Guid courseId, CancellationToken cancellationToken = default);
    Task<bool> DeleteSessionAsync(Guid id, CancellationToken cancellationToken = default);
}
