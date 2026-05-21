using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend_project.DTOs.LiveSession;

namespace backend_project.Services.Interfaces;

public interface ILiveSessionService
{
    Task<LiveSessionResponseDto> CreateSessionAsync(CreateLiveSessionDto createDto);
    Task<LiveSessionResponseDto> UpdateStatusAsync(Guid id, UpdateLiveSessionStatusDto statusDto);
    Task<IEnumerable<LiveSessionResponseDto>> GetSessionsForCourseAsync(Guid courseId);
    Task<bool> DeleteSessionAsync(Guid id);
}
