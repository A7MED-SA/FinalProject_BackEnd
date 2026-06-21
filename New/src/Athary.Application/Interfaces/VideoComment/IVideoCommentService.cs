using Athary.Application.DTOs.VideoComment;

namespace Athary.Application.Interfaces.VideoComment;

public interface IVideoCommentService
{
    Task<List<CommentResponseDto>> GetCommentsForVideoAsync(Guid videoId, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<CommentResponseDto> AddCommentAsync(Guid videoId, Guid userId, CreateCommentDto createDto, CancellationToken cancellationToken = default);
    Task<CommentResponseDto> UpdateCommentAsync(Guid commentId, Guid userId, CreateCommentDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteCommentAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
    Task<CommentLikeResponseDto> ToggleLikeAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
}
