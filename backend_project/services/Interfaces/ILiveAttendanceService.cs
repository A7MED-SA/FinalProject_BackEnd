using System;
using System.Threading.Tasks;

namespace backend_project.Services.Interfaces;

public interface ILiveAttendanceService
{
    Task<bool> JoinSessionAsync(Guid sessionId, Guid userId);
    Task<bool> LeaveSessionAsync(Guid sessionId, Guid userId);
    Task<int> GetAttendanceCountAsync(Guid sessionId);
}
