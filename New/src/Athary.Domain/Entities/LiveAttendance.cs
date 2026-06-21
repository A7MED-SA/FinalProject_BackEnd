namespace Athary.Domain.Entities;

public sealed class LiveAttendance : BaseEntity
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    public int? DurationMinutes { get; set; }

    public LiveSession Session { get; set; } = null!;
    public User User { get; set; } = null!;
}
