using Athary.Application.DTOs.Courses;

namespace Athary.Application.Interfaces.Courses;

public interface ICourseService
{
    Task<CourseDetailsDto> CreateCourseAsync(Guid instructorId, CreateCourseDto dto, CancellationToken cancellationToken = default);

    Task<CourseDetailsDto> UpdateCourseAsync(Guid courseId, Guid instructorId, UpdateCourseDto dto, CancellationToken cancellationToken = default);

    Task<CourseDetailsDto> GetCourseByIdAsync(Guid courseId, CancellationToken cancellationToken = default);

    Task<CourseRequirementDto> AddRequirementAsync(Guid courseId, Guid instructorId, AddRequirementDto dto, CancellationToken cancellationToken = default);

    Task RemoveRequirementAsync(Guid courseId, Guid requirementId, Guid instructorId, CancellationToken cancellationToken = default);

    Task<CourseLearningOutcomeDto> AddLearningOutcomeAsync(Guid courseId, Guid instructorId, AddLearningOutcomeDto dto, CancellationToken cancellationToken = default);

    Task RemoveLearningOutcomeAsync(Guid courseId, Guid outcomeId, Guid instructorId, CancellationToken cancellationToken = default);

    Task SubmitForReviewAsync(Guid courseId, Guid instructorId, CancellationToken cancellationToken = default);

    Task ApproveCourseAsync(Guid courseId, Guid adminId, CancellationToken cancellationToken = default);

    Task RejectCourseAsync(Guid courseId, Guid adminId, string reason, CancellationToken cancellationToken = default);

    Task<CourseDetailsDto> SetCourseImageAsync(Guid courseId, Guid fileId, Guid userId, CancellationToken cancellationToken = default);

    Task<CourseDetailsDto> RemoveCourseImageAsync(Guid courseId, Guid userId, CancellationToken cancellationToken = default);

    Task DeleteCourseAsync(Guid courseId, Guid instructorId, CancellationToken cancellationToken = default);

    Task ScheduleDeletionAsync(Guid courseId, Guid instructorId, DateTime scheduledDate, string? reason, CancellationToken cancellationToken = default);

    Task CancelScheduledDeletionAsync(Guid courseId, Guid instructorId, CancellationToken cancellationToken = default);

    Task<ScheduledDeletionStatusDto> GetScheduledDeletionStatusAsync(Guid courseId, CancellationToken cancellationToken = default);
}
