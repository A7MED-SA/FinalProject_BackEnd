using backend_project.Services.Interfaces;

namespace backend_project.Workers;

public class VideoProcessingWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VideoProcessingWorker> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30);
    private readonly int _batchSize = 5;

    public VideoProcessingWorker(
        IServiceProvider serviceProvider,
        ILogger<VideoProcessingWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Video Processing Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingVideosAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Video Processing Worker");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Video Processing Worker stopped");
    }

    private async Task ProcessPendingVideosAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var videoService = scope.ServiceProvider.GetRequiredService<IVideoProcessingService>();

        // Get pending videos
        var pendingVideos = await videoService.GetPendingVideosAsync(_batchSize);

        if (pendingVideos.Count == 0)
        {
            _logger.LogDebug("No pending videos to process");
            return;
        }

        _logger.LogInformation("Processing {Count} pending videos", pendingVideos.Count);

        foreach (var videoId in pendingVideos)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                _logger.LogInformation("Processing video {VideoId}", videoId);

                var result = await videoService.ProcessVideoAsync(videoId, stoppingToken);

                if (result.Success)
                {
                    _logger.LogInformation(
                        "Video {VideoId} processed successfully. Duration: {Duration}s",
                        videoId,
                        result.DurationSeconds);
                }
                else
                {
                    _logger.LogWarning(
                        "Video {VideoId} processing failed: {Error}",
                        videoId,
                        result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process video {VideoId}", videoId);
                await videoService.MarkVideoFailedAsync(videoId, ex.Message);
            }
        }
    }
}
