using backend_project.DTOs.EditRequest;
using backend_project.Helpers;

namespace backend_project.Services.Interfaces;

/// <summary>
/// Service for managing the course edit approval workflow.
/// Handles edit submissions, admin reviews, cancellations, and cleanup.
/// </summary>
public interface ICourseEditApprovalService
{
    /// <summary>
    /// Submit an edit. Automatically applies it or creates a pending request based on the edit policy.
    /// </summary>
    Task<EditResultDto> RequestEditAsync(
        Guid courseId,
        Guid instructorId,
        EditContext context,
        string? payloadJson);

    /// <summary>
    /// Get a paginated list of pending edit requests (for Admin dashboard).
    /// </summary>
    Task<PagedResult<EditRequestSummaryDto>> GetPendingRequestsAsync(EditRequestFilterDto filter);

    /// <summary>
    /// Get full details of an edit request including diff and student impact analysis.
    /// </summary>
    Task<EditRequestDetailDto> GetRequestDetailsAsync(Guid requestId);

    /// <summary>
    /// Admin approves or rejects an edit request.
    /// </summary>
    Task<EditResultDto> ReviewRequestAsync(
        Guid requestId,
        Guid adminId,
        bool approve,
        string? notes);

    /// <summary>
    /// Instructor cancels their own pending edit request.
    /// </summary>
    Task<bool> CancelRequestAsync(Guid requestId, Guid instructorId);

    /// <summary>
    /// Cleanup expired edit requests (called by BackgroundService).
    /// </summary>
    Task<int> CleanupExpiredRequestsAsync();
}
