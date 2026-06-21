using Athary.Application.Interfaces.LiveSession;
using Microsoft.Extensions.Logging;

namespace Athary.Infrastructure.Services.LiveSession;

public sealed class DefaultStreamingProvider : IStreamingProvider
{
    private readonly ILogger<DefaultStreamingProvider> _logger;

    public DefaultStreamingProvider(ILogger<DefaultStreamingProvider> logger)
    {
        _logger = logger;
    }

    public Task<StreamingRoomInfo> CreateRoomAsync(string title, int? maxAttendees, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating streaming room for '{Title}' (dev mode)", title);
        return Task.FromResult(new StreamingRoomInfo
        {
            IngestUrl = "rtmp://localhost:1935/live",
            StreamKey = Guid.NewGuid().ToString("N"),
            PlaybackUrl = $"https://stream.example.com/watch/{Guid.NewGuid():N}",
            BrowserStudioUrl = null
        });
    }

    public Task EndRoomAsync(string playbackUrl, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ending streaming room at {Url} (dev mode)", playbackUrl);
        return Task.CompletedTask;
    }
}
