using backend_project.DTOs.Communication;

namespace backend_project.Services.Interfaces;

public interface IReportService
{
    Task<ReportResponse> CreateAsync(Guid userId, CreateReportRequest request);
    Task<IEnumerable<ReportResponse>> GetPendingAsync(int page, int pageSize);
    Task<ReportResponse> ResolveAsync(Guid reportId, ResolveReportRequest request, Guid adminId);
}
