using backend_project.DTOs.Media;
using backend_project.Models;

namespace backend_project.Services.Interfaces;

public interface IMediaService
{
    /// <summary>
    /// Generate a presigned upload URL for the user
    /// </summary>
    Task<UploadUrlResponseDto> GenerateUploadUrlAsync(
        Guid userId,
        UploadUrlRequestDto request);

    /// <summary>
    /// Confirm upload completion and create DB record
    /// </summary>
    Task<MediaFileDto> ConfirmUploadAsync(
        Guid userId,
        ConfirmUploadDto request);

    /// <summary>
    /// Get a presigned view URL with permission checks
    /// </summary>
    Task<ViewUrlResponseDto> GetViewUrlAsync(
        Guid fileId,
        Guid? userId,
        IEnumerable<string> userRoles);

    /// <summary>
    /// Check if user has permission to access a file
    /// </summary>
    Task<bool> CheckAccessPermissionAsync(
        UploadedFile file,
        Guid? userId,
        IEnumerable<string> userRoles);
}
