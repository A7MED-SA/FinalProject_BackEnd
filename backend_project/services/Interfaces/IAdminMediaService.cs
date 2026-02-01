using backend_project.DTOs.Media;
using backend_project.Models;

namespace backend_project.Services.Interfaces;

public interface IAdminMediaService
{
    /// <summary>
    /// List media files with filters
    /// </summary>
    Task<PagedResult<MediaFileDto>> ListMediaAsync(MediaFilterDto filter);

    /// <summary>
    /// Get detailed media file info
    /// </summary>
    Task<MediaDetailDto> GetMediaDetailsAsync(Guid fileId);

    /// <summary>
    /// Soft delete a media file
    /// </summary>
    Task SoftDeleteAsync(Guid fileId, Guid adminId);

    /// <summary>
    /// Restore a soft-deleted media file
    /// </summary>
    Task RestoreAsync(Guid fileId, Guid adminId);

    /// <summary>
    /// Permanently delete from DB and MinIO
    /// </summary>
    Task PermanentDeleteAsync(Guid fileId, Guid adminId);

    /// <summary>
    /// Get storage statistics
    /// </summary>
    Task<StorageStatsDto> GetStorageStatsAsync();
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
