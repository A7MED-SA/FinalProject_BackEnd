using backend_project.DTOs.Refund;
using backend_project.Models;

namespace backend_project.Services.Interfaces;

public interface IRefundService
{
    Task<RefundResponseDto> RequestRefundAsync(Guid userId, RequestRefundRequest request);
    Task<IEnumerable<RefundResponseDto>> GetUserRefundsAsync(Guid userId);
    Task<IEnumerable<RefundResponseDto>> GetAllRefundsAsync(Models.RefundStatus? status = null);
    Task<RefundResponseDto> ApproveRefundAsync(Guid adminId, ProcessRefundRequest request);
    Task<RefundResponseDto> RejectRefundAsync(Guid adminId, ProcessRefundRequest request);
}
