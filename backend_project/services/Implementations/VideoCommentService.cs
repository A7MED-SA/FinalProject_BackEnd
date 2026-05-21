using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.VideoComment;
using backend_project.Models;
using backend_project.Services.Interfaces;

namespace backend_project.Services.Implementations;

public class VideoCommentService : IVideoCommentService
{
    private readonly ApplicationDbContext _context;

    public VideoCommentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CommentResponseDto>> GetCommentsForVideoAsync(Guid videoId, Guid? userId = null)
    {
        var comments = await _context.VideoComments
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Replies.Where(r => r.DeletedAt == null))
                .ThenInclude(r => r.User)
            .Where(c => c.VideoId == videoId && c.ParentCommentId == null && c.DeletedAt == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return comments.Select(c => MapToDto(c, userId));
    }

    public async Task<CommentResponseDto> AddCommentAsync(Guid videoId, Guid userId, CreateCommentDto createDto)
    {
        var video = await _context.Videos.AnyAsync(v => v.Id == videoId);
        if (!video)
            throw new KeyNotFoundException("Video not found.");

        Guid? resolvedParentId = createDto.ParentCommentId;

        if (resolvedParentId.HasValue)
        {
            var parentComment = await _context.VideoComments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == resolvedParentId.Value && c.DeletedAt == null);

            if (parentComment == null)
                throw new KeyNotFoundException("Parent comment not found.");

            if (parentComment.ParentCommentId.HasValue)
            {
                resolvedParentId = parentComment.ParentCommentId;
            }
        }

        var comment = new VideoComment
        {
            Id = Guid.NewGuid(),
            VideoId = videoId,
            UserId = userId,
            ParentCommentId = resolvedParentId,
            Content = createDto.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.VideoComments.Add(comment);
        await _context.SaveChangesAsync();

        return await GetSingleCommentAsync(comment.Id, userId);
    }

    public async Task<CommentResponseDto> UpdateCommentAsync(Guid commentId, Guid userId, CreateCommentDto updateDto)
    {
        var comment = await _context.VideoComments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.DeletedAt == null);

        if (comment == null)
            throw new KeyNotFoundException("Comment not found.");

        if (comment.UserId != userId)
            throw new UnauthorizedAccessException("You can only edit your own comments.");

        comment.Content = updateDto.Content;
        comment.IsEdited = true;
        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(comment, userId);
    }

    public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _context.VideoComments
            .FirstOrDefaultAsync(c => c.Id == commentId && c.DeletedAt == null);

        if (comment == null)
            return false;

        comment.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<CommentLikeResponseDto> ToggleLikeAsync(Guid commentId, Guid userId)
    {
        var comment = await _context.VideoComments
            .FirstOrDefaultAsync(c => c.Id == commentId && c.DeletedAt == null);

        if (comment == null)
            throw new KeyNotFoundException("Comment not found.");

        var existingLike = await _context.CommentLikes
            .FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == userId);

        if (existingLike != null)
        {
            _context.CommentLikes.Remove(existingLike);
            comment.LikesCount = Math.Max(0, comment.LikesCount - 1);
            await _context.SaveChangesAsync();

            return new CommentLikeResponseDto
            {
                CommentId = commentId,
                LikesCount = comment.LikesCount,
                IsLikedByCurrentUser = false
            };
        }
        else
        {
            var like = new CommentLike
            {
                Id = Guid.NewGuid(),
                CommentId = commentId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.CommentLikes.Add(like);
            comment.LikesCount++;
            await _context.SaveChangesAsync();

            return new CommentLikeResponseDto
            {
                CommentId = commentId,
                LikesCount = comment.LikesCount,
                IsLikedByCurrentUser = true
            };
        }
    }

    private async Task<CommentResponseDto> GetSingleCommentAsync(Guid commentId, Guid? userId)
    {
        var comment = await _context.VideoComments
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Replies.Where(r => r.DeletedAt == null))
                .ThenInclude(r => r.User)
            .FirstAsync(c => c.Id == commentId);

        return MapToDto(comment, userId);
    }

    private CommentResponseDto MapToDto(VideoComment comment, Guid? currentUserId)
    {
        return new CommentResponseDto
        {
            Id = comment.Id,
            VideoId = comment.VideoId,
            UserId = comment.UserId,
            UserName = comment.User?.FullName ?? "Unknown",
            Content = comment.Content,
            LikesCount = comment.LikesCount,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            ParentCommentId = comment.ParentCommentId,
            Replies = comment.Replies?.Select(r => new CommentResponseDto
            {
                Id = r.Id,
                VideoId = r.VideoId,
                UserId = r.UserId,
                UserName = r.User?.FullName ?? "Unknown",
                Content = r.Content,
                LikesCount = r.LikesCount,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                ParentCommentId = r.ParentCommentId
            }).ToList()
        };
    }
}
