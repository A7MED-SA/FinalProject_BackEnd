using Athary.Application.DTOs.Media;

namespace Athary.Application.Interfaces.Media;

public interface IAdminMediaService
{
    Task<PagedResult<MediaFileDto>> ListMediaAsync(MediaFilterDto filter, CancellationToken cancellationToken = default);

    Task<MediaDetailDto> GetMediaDetailsAsync(Guid fileId, CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(Guid fileId, Guid adminId, CancellationToken cancellationToken = default);

    Task RestoreAsync(Guid fileId, Guid adminId, CancellationToken cancellationToken = default);

    Task PermanentDeleteAsync(Guid fileId, Guid adminId, CancellationToken cancellationToken = default);

    Task<StorageStatsDto> GetStorageStatsAsync(CancellationToken cancellationToken = default);
}

public sealed record PagedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
