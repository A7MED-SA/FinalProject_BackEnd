namespace Athary.Application.Interfaces.Media;

public interface IVideoProcessingService
{
    Task QueueVideoForProcessingAsync(Guid videoId, CancellationToken cancellationToken = default);

    Task<VideoProcessingResult> ProcessVideoAsync(Guid videoId, CancellationToken cancellationToken);

    Task<List<Guid>> GetPendingVideosAsync(int batchSize = 10, CancellationToken cancellationToken = default);

    Task MarkVideoFailedAsync(Guid videoId, string reason, CancellationToken cancellationToken = default);
}

public sealed record VideoProcessingResult
{
    public bool Success { get; init; }
    public int? DurationSeconds { get; init; }
    public string? ErrorMessage { get; init; }
    public VideoMetadata? Metadata { get; init; }
}

public sealed record VideoMetadata
{
    public int DurationSeconds { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public string Codec { get; init; } = string.Empty;
    public double Bitrate { get; init; }
    public double FrameRate { get; init; }
}
