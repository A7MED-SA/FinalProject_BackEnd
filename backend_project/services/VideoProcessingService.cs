using backend_project.Data;
using backend_project.Models;
using backend_project.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_project.Services;

public class VideoProcessingService : IVideoProcessingService
{
    private readonly ApplicationDbContext _context;
    private readonly IObjectStorage _objectStorage;
    private readonly ILogger<VideoProcessingService> _logger;

    public VideoProcessingService(
        ApplicationDbContext context,
        IObjectStorage objectStorage,
        ILogger<VideoProcessingService> logger)
    {
        _context = context;
        _objectStorage = objectStorage;
        _logger = logger;
    }

    public async Task QueueVideoForProcessingAsync(Guid videoId)
    {
        var video = await _context.Videos.FindAsync(videoId)
            ?? throw new KeyNotFoundException("Video not found");

        video.Status = VideoStatus.Processing;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Video {VideoId} queued for processing", videoId);
    }

    public async Task<List<Guid>> GetPendingVideosAsync(int batchSize = 10)
    {
        return await _context.Videos
            .Where(v => v.Status == VideoStatus.Processing)
            .OrderBy(v => v.Id) // Deterministic order
            .Take(batchSize)
            .Select(v => v.Id)
            .ToListAsync();
    }

    public async Task<VideoProcessingResult> ProcessVideoAsync(
        Guid videoId,
        CancellationToken cancellationToken)
    {
        var video = await _context.Videos
            .Include(v => v.VideoFile)
            .FirstOrDefaultAsync(v => v.Id == videoId, cancellationToken);

        if (video == null)
        {
            return new VideoProcessingResult
            {
                Success = false,
                ErrorMessage = "Video not found"
            };
        }

        if (video.Status != VideoStatus.Processing)
        {
            _logger.LogWarning("Video {VideoId} is not in Processing state", videoId);
            return new VideoProcessingResult
            {
                Success = false,
                ErrorMessage = "Video is not in Processing state"
            };
        }

        try
        {
            // Step 1: Verify file exists in MinIO
            var exists = await _objectStorage.ObjectExistsAsync(
                video.VideoFile.Bucket,
                video.VideoFile.FilePath);

            if (!exists)
            {
                await MarkVideoFailedAsync(videoId, "File not found in storage");
                return new VideoProcessingResult
                {
                    Success = false,
                    ErrorMessage = "File not found in storage"
                };
            }

            // Step 2: Get file metadata
            var metadata = await _objectStorage.GetObjectMetadataAsync(
                video.VideoFile.Bucket,
                video.VideoFile.FilePath);

            if (metadata == null)
            {
                await MarkVideoFailedAsync(videoId, "Failed to get file metadata");
                return new VideoProcessingResult
                {
                    Success = false,
                    ErrorMessage = "Failed to get file metadata"
                };
            }

            // Step 3: Extract video metadata (stub - in production use FFprobe)
            var videoMetadata = await ExtractVideoMetadataAsync(
                video.VideoFile.Bucket,
                video.VideoFile.FilePath,
                cancellationToken);

            // Step 4: Update video record
            video.DurationSeconds = videoMetadata?.DurationSeconds ?? 0;
            video.Status = VideoStatus.Ready;
            video.VideoFile.Status = FileStatus.Ready;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Video {VideoId} processed successfully. Duration: {Duration}s",
                videoId,
                video.DurationSeconds);

            return new VideoProcessingResult
            {
                Success = true,
                DurationSeconds = video.DurationSeconds,
                Metadata = videoMetadata
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing video {VideoId}", videoId);
            await MarkVideoFailedAsync(videoId, ex.Message);

            return new VideoProcessingResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task MarkVideoFailedAsync(Guid videoId, string reason)
    {
        var video = await _context.Videos
            .Include(v => v.VideoFile)
            .FirstOrDefaultAsync(v => v.Id == videoId);

        if (video != null)
        {
            video.Status = VideoStatus.Failed;
            video.VideoFile.Status = FileStatus.Failed;
            await _context.SaveChangesAsync();

            _logger.LogError("Video {VideoId} marked as failed: {Reason}", videoId, reason);
        }
    }

    /// <summary>
    /// Stub for video metadata extraction.
    /// In production, use FFprobe or similar tool.
    /// </summary>
    private async Task<VideoMetadata?> ExtractVideoMetadataAsync(
        string bucket,
        string objectKey,
        CancellationToken cancellationToken)
    {
        // TODO: Implement actual video metadata extraction using FFprobe
        // For now, return stub data based on file size estimation

        var metadata = await _objectStorage.GetObjectMetadataAsync(bucket, objectKey);
        if (metadata == null)
            return null;

        // Estimate duration based on file size (rough approximation)
        // Assuming ~1MB per 10 seconds of 720p video
        var estimatedDuration = (int)(metadata.SizeBytes / (1024 * 1024) * 10);
        estimatedDuration = Math.Max(estimatedDuration, 1); // At least 1 second

        return new VideoMetadata
        {
            DurationSeconds = estimatedDuration,
            Width = 1280,
            Height = 720,
            Codec = "h264",
            Bitrate = 2500000,
            FrameRate = 30
        };
    }
}
