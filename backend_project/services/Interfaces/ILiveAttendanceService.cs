using System;
using System.Threading.Tasks;

namespace backend_project.Services.Interfaces;

public interface ILiveAttendanceService
{
    Task<bool> MarkAttendanceAsync(Guid sessionId, Guid userId);
    Task<int> GetSessionAttendanceCountAsync(Guid sessionId);
    Task<bool> HasUserAttendedAsync(Guid sessionId, Guid userId);
}
