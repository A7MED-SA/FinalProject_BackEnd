using backend_project.Services.Interfaces;
using Microsoft.Extensions.Hosting;

namespace backend_project.Workers;

public class VideoProcessingWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VideoProcessingWorker> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30);
    private readonly int _batchSize = 5;
    private int _consecutiveEmptyPolls = 0;
    private readonly int _maxEmptyPollsBeforeLongSleep = 5;
    private readonly TimeSpan _longSleepInterval = TimeSpan.FromMinutes(2);

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
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Video Processing Worker cancellation requested");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Video Processing Worker");
                // Don't stop on error, continue processing
            }

            // Adaptive polling: sleep longer if no pending videos
            var sleepInterval = _consecutiveEmptyPolls >= _maxEmptyPollsBeforeLongSleep 
                ? _longSleepInterval 
                : _pollingInterval;

            await Task.Delay(sleepInterval, stoppingToken);
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
            _consecutiveEmptyPolls++;
            _logger.LogDebug("No pending videos to process (empty poll count: {Count})", _consecutiveEmptyPolls);
            return;
        }

        _consecutiveEmptyPolls = 0; // Reset counter
        _logger.LogInformation("Processing {Count} pending videos", pendingVideos.Count);

        foreach (var videoId in pendingVideos)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Processing cancelled, stopping current batch");
                break;
            }

            try
            {
                _logger.LogInformation("Starting processing for video {VideoId}", videoId);

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
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Processing for video {VideoId} was cancelled", videoId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process video {VideoId}", videoId);
                try
                {
                    await videoService.MarkVideoFailedAsync(videoId, ex.Message);
                }
                catch (Exception markFailedEx)
                {
                    _logger.LogError(markFailedEx, "Failed to mark video {VideoId} as failed", videoId);
                }
            }
        }
    }
}