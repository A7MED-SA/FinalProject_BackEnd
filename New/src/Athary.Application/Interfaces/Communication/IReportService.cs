using Athary.Application.DTOs.Communication;

namespace Athary.Application.Interfaces.Communication;

public interface IReportService
{
    Task<ReportResponse> CreateAsync(Guid userId, CreateReportRequest request, CancellationToken cancellationToken = default);
    Task<ReportListResponse> GetPendingAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ReportResponse> ResolveAsync(Guid reportId, ResolveReportRequest request, Guid adminId, CancellationToken cancellationToken = default);
}
