using Athary.Application.DTOs.Courses;

namespace Athary.Application.Interfaces.Courses;

public interface IVideoService
{
    Task<VideoResponseDto> GetVideoAsync(Guid id, CancellationToken cancellationToken = default);

    Task<VideoResponseDto> CreateVideoAsync(Guid courseId, CreateVideoDto createDto, CancellationToken cancellationToken = default);

    Task<VideoResponseDto> UpdateVideoAsync(Guid id, UpdateVideoDto updateDto, CancellationToken cancellationToken = default);

    Task<bool> DeleteVideoAsync(Guid id, CancellationToken cancellationToken = default);
}
