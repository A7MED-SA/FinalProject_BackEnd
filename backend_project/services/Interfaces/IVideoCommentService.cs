using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend_project.DTOs.VideoComment;

namespace backend_project.Services.Interfaces;

public interface IVideoCommentService
{
    Task<IEnumerable<CommentResponseDto>> GetCommentsForVideoAsync(Guid videoId, Guid? userId = null);
    Task<CommentResponseDto> AddCommentAsync(Guid videoId, Guid userId, CreateCommentDto createDto);
    Task<CommentResponseDto> UpdateCommentAsync(Guid commentId, Guid userId, CreateCommentDto updateDto);
    Task<bool> DeleteCommentAsync(Guid commentId, Guid userId); // Soft delete
    
    Task<CommentLikeResponseDto> ToggleLikeAsync(Guid commentId, Guid userId);
}
