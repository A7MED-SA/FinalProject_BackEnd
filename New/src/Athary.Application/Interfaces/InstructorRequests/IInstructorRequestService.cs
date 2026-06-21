using Athary.Application.DTOs.InstructorRequests.Requests;
using Athary.Application.DTOs.InstructorRequests.Responses;

namespace Athary.Application.Interfaces.InstructorRequests;

public interface IInstructorRequestService
{
    Task<bool> CanSubmitRequestAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<InstructorRequestDto> SubmitRequestAsync(Guid userId, SubmitInstructorRequestDto dto, CancellationToken cancellationToken = default);
    Task<InstructorRequestDto> UpdateRequestAsync(Guid requestId, Guid userId, UpdateInstructorRequestDto dto, CancellationToken cancellationToken = default);
    Task<bool> AddDocumentAsync(Guid requestId, Guid userId, AddDocumentToRequestDto dto, CancellationToken cancellationToken = default);
    Task<List<InstructorRequestDto>> GetMyRequestsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<InstructorRequestDetailDto> GetMyRequestByIdAsync(Guid userId, Guid requestId, CancellationToken cancellationToken = default);
    Task<bool> CancelRequestAsync(Guid userId, Guid requestId, CancellationToken cancellationToken = default);

    Task<List<InstructorRequestDto>> GetPendingRequestsAsync(CancellationToken cancellationToken = default);
    Task<InstructorRequestDetailDto> GetRequestByIdAsync(Guid requestId, CancellationToken cancellationToken = default);
    Task<InstructorRequestDto> ProcessRequestAsync(Guid requestId, Guid adminId, ProcessInstructorRequestDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteRequestAsync(Guid requestId, CancellationToken cancellationToken = default);
}
