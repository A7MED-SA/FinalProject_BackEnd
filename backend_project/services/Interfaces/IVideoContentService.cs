using System;
using System.Threading.Tasks;
using backend_project.DTOs.Video;

namespace backend_project.Services.Interfaces;

public interface IVideoContentService
{
    Task<VideoResponseDto> GetVideoAsync(Guid id);
    Task<VideoResponseDto> CreateVideoAsync(CreateVideoDto createDto);
    Task<VideoResponseDto> UpdateVideoAsync(Guid id, UpdateVideoDto updateDto);
    Task<bool> DeleteVideoAsync(Guid id); // Handles SectionItem removal
}
