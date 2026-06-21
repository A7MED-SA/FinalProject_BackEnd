using Athary.Application.DTOs.VideoComment;
using Athary.Application.Interfaces.VideoComment;
using Athary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.VideoComment;

public sealed class VideoCommentService : IVideoCommentService
{
    private readonly ApplicationDbContext _context;

    public VideoCommentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CommentResponseDto>> GetCommentsForVideoAsync(Guid videoId, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var comments = await _context.VideoComments
            .AsNoTracking()
            .Include(vc => vc.User)
            .Include(vc => vc.Replies)
                .ThenInclude(r => r.User)
            .Where(vc => vc.VideoId == videoId && vc.ParentCommentId == null)
            .OrderByDescending(vc => vc.CreatedAt)
            .ToListAsync(cancellationToken);

        return comments.Select(c => MapToDto(c, userId)).ToList();
    }

    public async Task<CommentResponseDto> AddCommentAsync(Guid videoId, Guid userId, CreateCommentDto createDto, CancellationToken cancellationToken = default)
    {
        var videoExists = await _context.Videos.AnyAsync(v => v.Id == videoId, cancellationToken);
        if (!videoExists)
            throw new KeyNotFoundException("Video not found.");

        Guid? parentId = createDto.ParentCommentId;

        if (parentId.HasValue)
        {
            var parent = await _context.VideoComments
                .FirstOrDefaultAsync(vc => vc.Id == parentId.Value && vc.VideoId == videoId, cancellationToken);

            if (parent == null)
                throw new KeyNotFoundException("Parent comment not found.");

            if (parent.ParentCommentId.HasValue)
                parentId = parent.ParentCommentId;
        }

        var comment = new Athary.Domain.Entities.VideoComment
        {
            VideoId = videoId,
            UserId = userId,
            ParentCommentId = parentId,
            Content = createDto.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.VideoComments.Add(comment);
        await _context.SaveChangesAsync(cancellationToken);

        await _context.Entry(comment).Reference(c => c.User).LoadAsync(cancellationToken);

        return MapToDto(comment, userId);
    }

    public async Task<CommentResponseDto> UpdateCommentAsync(Guid commentId, Guid userId, CreateCommentDto updateDto, CancellationToken cancellationToken = default)
    {
        var comment = await _context.VideoComments
            .Include(vc => vc.User)
            .FirstOrDefaultAsync(vc => vc.Id == commentId && vc.UserId == userId, cancellationToken);

        if (comment == null)
            throw new KeyNotFoundException("Comment not found or you are not the owner.");

        comment.Content = updateDto.Content;
        comment.IsEdited = true;
        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(comment, userId);
    }

    public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default)
    {
        var comment = await _context.VideoComments
            .FirstOrDefaultAsync(vc => vc.Id == commentId && vc.UserId == userId, cancellationToken);

        if (comment == null)
            return false;

        comment.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<CommentLikeResponseDto> ToggleLikeAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default)
    {
        var comment = await _context.VideoComments
            .FirstOrDefaultAsync(vc => vc.Id == commentId, cancellationToken);

        if (comment == null)
            throw new KeyNotFoundException("Comment not found.");

        var existingLike = await _context.CommentLikes
            .FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == userId, cancellationToken);

        bool isLiked;
        if (existingLike != null)
        {
            _context.CommentLikes.Remove(existingLike);
            comment.LikesCount = Math.Max(0, comment.LikesCount - 1);
            isLiked = false;
        }
        else
        {
            _context.CommentLikes.Add(new Athary.Domain.Entities.CommentLike
            {
                CommentId = commentId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
            comment.LikesCount++;
            isLiked = true;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new CommentLikeResponseDto
        {
            CommentId = commentId,
            LikesCount = comment.LikesCount,
            IsLikedByCurrentUser = isLiked
        };
    }

    private static CommentResponseDto MapToDto(Athary.Domain.Entities.VideoComment comment, Guid? currentUserId)
    {
        return new CommentResponseDto
        {
            Id = comment.Id,
            VideoId = comment.VideoId,
            UserId = comment.UserId,
            UserName = $"{comment.User.FirstName} {comment.User.LastName}",
            Content = comment.Content,
            IsEdited = comment.IsEdited,
            LikesCount = comment.LikesCount,
            RepliesCount = comment.Replies?.Count ?? 0,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            ParentCommentId = comment.ParentCommentId,
            Replies = comment.Replies?.Select(r => new CommentResponseDto
            {
                Id = r.Id,
                VideoId = r.VideoId,
                UserId = r.UserId,
                UserName = $"{r.User.FirstName} {r.User.LastName}",
                Content = r.Content,
                IsEdited = r.IsEdited,
                LikesCount = r.LikesCount,
                RepliesCount = 0,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                ParentCommentId = r.ParentCommentId
            }).ToList()
        };
    }
}
