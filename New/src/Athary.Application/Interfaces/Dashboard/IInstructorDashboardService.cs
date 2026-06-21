using Athary.Application.DTOs.Dashboard;

namespace Athary.Application.Interfaces.Dashboard;

public interface IInstructorDashboardService
{
    Task<InstructorOverviewDto> GetInstructorOverviewAsync(Guid instructorId, CancellationToken cancellationToken = default);
    Task<List<ManagementCourseDto>> GetInstructorCoursesAsync(Guid instructorId, CancellationToken cancellationToken = default);
    Task<InstructorDashboardRevenueDto> GetInstructorRevenueAsync(Guid instructorId, int months = 12, CancellationToken cancellationToken = default);
    Task<InstructorDashboardStudentsDto> GetInstructorStudentsAsync(Guid instructorId, CancellationToken cancellationToken = default);
    Task<List<PendingEditRequestDto>> GetInstructorPendingRequestsAsync(Guid instructorId, CancellationToken cancellationToken = default);
    Task<List<ReviewSummaryDto>> GetInstructorRecentReviewsAsync(Guid instructorId, int limit = 10, CancellationToken cancellationToken = default);
}
