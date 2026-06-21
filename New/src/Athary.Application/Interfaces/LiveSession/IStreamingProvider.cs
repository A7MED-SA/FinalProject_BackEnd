namespace Athary.Application.Interfaces.LiveSession;

public sealed record StreamingRoomInfo
{
    public string IngestUrl { get; init; } = string.Empty;
    public string StreamKey { get; init; } = string.Empty;
    public string PlaybackUrl { get; init; } = string.Empty;
    public string? BrowserStudioUrl { get; init; }
}

public interface IStreamingProvider
{
    Task<StreamingRoomInfo> CreateRoomAsync(string title, int? maxAttendees, CancellationToken cancellationToken = default);
    Task EndRoomAsync(string playbackUrl, CancellationToken cancellationToken = default);
}
