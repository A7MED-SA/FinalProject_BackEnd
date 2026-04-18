using backend_project.Models;

namespace backend_project.Services.Interfaces;

public interface IVideoProcessingService
{
    /// <summary>
    /// Queue a video for processing
    /// </summary>
    Task QueueVideoForProcessingAsync(Guid videoId);

    /// <summary>
    /// Process a single video (called by background worker)
    /// </summary>
    Task<VideoProcessingResult> ProcessVideoAsync(Guid videoId, CancellationToken cancellationToken);

    /// <summary>
    /// Get pending videos for processing
    /// </summary>
    Task<List<Guid>> GetPendingVideosAsync(int batchSize = 10);

    /// <summary>
    /// Mark video as failed
    /// </summary>
    Task MarkVideoFailedAsync(Guid videoId, string reason);
}

public class VideoProcessingResult
{
    public bool Success { get; set; }
    public int? DurationSeconds { get; set; }
    public string? ErrorMessage { get; set; }
    public VideoMetadata? Metadata { get; set; }
}

public class VideoMetadata
{
    public int DurationSeconds { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Codec { get; set; } = string.Empty;
    public double Bitrate { get; set; }
    public double FrameRate { get; set; }
}
