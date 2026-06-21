using Athary.Application.Common;
using Athary.Application.DTOs.Courses;
using Athary.Domain.Enums;

namespace Athary.Application.Interfaces.Courses;

public interface ICourseEditApprovalService
{
    Task<EditResultDto> RequestEditAsync(Guid courseId, Guid instructorId, EditRequestType targetType, EditOperation operation, string? propertyName, Guid? targetEntityId, string? payloadJson, CancellationToken cancellationToken = default);

    Task<PagedList<EditRequestSummaryDto>> GetPendingRequestsAsync(EditRequestFilterDto filter, CancellationToken cancellationToken = default);

    Task<EditRequestDetailDto> GetRequestDetailsAsync(Guid requestId, CancellationToken cancellationToken = default);

    Task<EditResultDto> ReviewRequestAsync(Guid requestId, Guid adminId, bool approve, string? notes, CancellationToken cancellationToken = default);

    Task<bool> CancelRequestAsync(Guid requestId, Guid instructorId, CancellationToken cancellationToken = default);

    Task<int> CleanupExpiredRequestsAsync(CancellationToken cancellationToken = default);
}
