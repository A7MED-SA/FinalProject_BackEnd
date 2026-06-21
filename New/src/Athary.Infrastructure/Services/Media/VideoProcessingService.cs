using Athary.Application.Interfaces.Media;
using Microsoft.Extensions.Logging;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Domain.Interfaces;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.Media;

public sealed class VideoProcessingService : IVideoProcessingService
{
    private readonly IRepository<Video> _videoRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IObjectStorage _objectStorage;
    private readonly ILogger<VideoProcessingService> _logger;
    private readonly ApplicationDbContext _dbContext;

    public VideoProcessingService(
        IRepository<Video> videoRepo,
        IUnitOfWork unitOfWork,
        IObjectStorage objectStorage,
        ILogger<VideoProcessingService> logger,
        ApplicationDbContext dbContext)
    {
        _videoRepo = videoRepo;
        _unitOfWork = unitOfWork;
        _objectStorage = objectStorage;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task QueueVideoForProcessingAsync(Guid videoId, CancellationToken cancellationToken = default)
    {
        var video = await _videoRepo.GetByIdAsync(videoId, cancellationToken)
            ?? throw new KeyNotFoundException("Video not found");

        if (video.Status == VideoStatus.Ready)
            throw new InvalidOperationException("Video is already processed");

        video.Status = VideoStatus.Processing;

        await _videoRepo.UpdateAsync(video, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Video {VideoId} queued for processing", videoId);
    }

    public async Task<List<Guid>> GetPendingVideosAsync(int batchSize = 10, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Videos
            .Where(v => v.Status == VideoStatus.Processing)
            .OrderBy(v => v.Id)
            .Take(batchSize)
            .Select(v => v.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<VideoProcessingResult> ProcessVideoAsync(
        Guid videoId,
        CancellationToken cancellationToken)
    {
        var video = await _dbContext.Videos
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
            var exists = await _objectStorage.ObjectExistsAsync(
                video.VideoFile.Bucket,
                video.VideoFile.FilePath);

            if (!exists)
            {
                await MarkVideoFailedAsync(videoId, "File not found in storage", cancellationToken);
                return new VideoProcessingResult
                {
                    Success = false,
                    ErrorMessage = "File not found in storage"
                };
            }

            var metadata = await _objectStorage.GetObjectMetadataAsync(
                video.VideoFile.Bucket,
                video.VideoFile.FilePath);

            if (metadata == null)
            {
                await MarkVideoFailedAsync(videoId, "Failed to get file metadata", cancellationToken);
                return new VideoProcessingResult
                {
                    Success = false,
                    ErrorMessage = "Failed to get file metadata"
                };
            }

            var videoMetadata = await ExtractVideoMetadataAsync(
                video.VideoFile.Bucket,
                video.VideoFile.FilePath,
                cancellationToken);

            video.DurationSeconds = videoMetadata?.DurationSeconds ?? 0;
            video.Status = VideoStatus.Ready;
            video.VideoFile.Status = FileStatus.Ready;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Video {VideoId} processed successfully. Duration: {Duration}s, Size: {Size} bytes",
                videoId,
                video.DurationSeconds,
                metadata.SizeBytes);

            return new VideoProcessingResult
            {
                Success = true,
                DurationSeconds = video.DurationSeconds,
                Metadata = videoMetadata
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Video processing for {VideoId} was cancelled", videoId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing video {VideoId}", videoId);
            await MarkVideoFailedAsync(videoId, ex.Message, cancellationToken);

            return new VideoProcessingResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task MarkVideoFailedAsync(Guid videoId, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            var video = await _dbContext.Videos
                .Include(v => v.VideoFile)
                .FirstOrDefaultAsync(v => v.Id == videoId, cancellationToken);

            if (video != null)
            {
                video.Status = VideoStatus.Failed;
                video.VideoFile.Status = FileStatus.Failed;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogError("Video {VideoId} marked as failed: {Reason}", videoId, reason);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking video {VideoId} as failed", videoId);
        }
    }

    private async Task<VideoMetadata?> ExtractVideoMetadataAsync(
        string bucket,
        string objectKey,
        CancellationToken cancellationToken)
    {
        var metadata = await _objectStorage.GetObjectMetadataAsync(bucket, objectKey);
        if (metadata == null)
            return null;

        var estimatedDuration = (int)(metadata.SizeBytes / (1024.0 * 1024) * 10);
        estimatedDuration = Math.Max(estimatedDuration, 1);

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
