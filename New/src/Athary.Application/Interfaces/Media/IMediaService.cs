using Athary.Application.DTOs.Media;
using Athary.Domain.Entities;

namespace Athary.Application.Interfaces.Media;

public interface IMediaService
{
    Task<UploadUrlResponseDto> GenerateUploadUrlAsync(
        Guid userId,
        UploadUrlRequestDto request,
        CancellationToken cancellationToken = default);

    Task<MediaFileDto> ConfirmUploadAsync(
        Guid userId,
        ConfirmUploadDto request,
        CancellationToken cancellationToken = default);

    Task<ViewUrlResponseDto> GetViewUrlAsync(
        Guid fileId,
        Guid? userId,
        IEnumerable<string> userRoles,
        CancellationToken cancellationToken = default);

    Task<bool> CheckAccessPermissionAsync(
        UploadedFile file,
        Guid? userId,
        IEnumerable<string> userRoles,
        CancellationToken cancellationToken = default);
}
