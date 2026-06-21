namespace Athary.Application.Interfaces.LiveSession;

public interface ILiveAttendanceService
{
    Task<bool> JoinSessionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> LeaveSessionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetAttendanceCountAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
