using System;
using System.Threading.Tasks;
using backend_project.DTOs.LiveSession;

namespace backend_project.Services.Interfaces;

public interface ILiveSessionService
{
    Task<LiveSessionResponseDto> GetSessionAsync(Guid id);
    Task<LiveSessionResponseDto> CreateSessionAsync(CreateLiveSessionDto createDto);
    Task<LiveSessionResponseDto> UpdateSessionAsync(Guid id, CreateLiveSessionDto updateDto); // Update schedule/details
    Task<LiveSessionResponseDto> UpdateSessionStatusAsync(Guid id, UpdateLiveSessionStatusDto statusDto); // Update status/recording
    Task<bool> DeleteSessionAsync(Guid id); // Handles SectionItem removal
}
