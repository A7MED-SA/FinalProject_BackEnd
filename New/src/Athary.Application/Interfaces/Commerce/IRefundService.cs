using Athary.Application.DTOs.Commerce;
using Athary.Domain.Enums;

namespace Athary.Application.Interfaces.Commerce;

public interface IRefundService
{
    Task<RefundResponseDto> RequestRefundAsync(Guid userId, RequestRefundRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<RefundResponseDto>> GetUserRefundsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RefundResponseDto>> GetAllRefundsAsync(RefundStatus? status = null, CancellationToken cancellationToken = default);
    Task<RefundResponseDto> ApproveRefundAsync(Guid adminId, ProcessRefundRequest request, CancellationToken cancellationToken = default);
    Task<RefundResponseDto> RejectRefundAsync(Guid adminId, ProcessRefundRequest request, CancellationToken cancellationToken = default);
}
