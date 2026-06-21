using Athary.Application.DTOs.Courses;
using Athary.Domain.Enums;

namespace Athary.Application.Interfaces.Courses;

public interface ISectionService
{
    Task<SectionDto> CreateSectionAsync(Guid courseId, Guid instructorId, CreateSectionDto dto, CancellationToken cancellationToken = default);

    Task<SectionDto> UpdateSectionAsync(Guid sectionId, Guid instructorId, UpdateSectionDto dto, CancellationToken cancellationToken = default);

    Task<SectionDto> GetSectionByIdAsync(Guid sectionId, CancellationToken cancellationToken = default);

    Task DeleteSectionAsync(Guid sectionId, Guid instructorId, CancellationToken cancellationToken = default);

    Task<List<SectionDto>> GetSectionsForCourseAsync(Guid courseId, CancellationToken cancellationToken = default);

    Task ReorderSectionsAsync(Guid courseId, Guid instructorId, ReorderRequestDto dto, CancellationToken cancellationToken = default);

    Task<SectionItemDto> AddItemToSectionAsync(Guid sectionId, Guid instructorId, CreateSectionItemDto dto, CancellationToken cancellationToken = default);

    Task<SectionItemDto> UpdateSectionItemAsync(Guid itemId, Guid instructorId, UpdateSectionItemDto dto, CancellationToken cancellationToken = default);

    Task DeleteSectionItemAsync(Guid itemId, Guid instructorId, CancellationToken cancellationToken = default);

    Task ReorderSectionItemsAsync(Guid sectionId, Guid instructorId, ReorderRequestDto dto, CancellationToken cancellationToken = default);
}

public interface IEnrollmentService
{
    Task<EnrollmentResponseDto> EnrollUserAsync(CreateEnrollmentDto createDto, CancellationToken cancellationToken = default);

    Task<EnrollmentDetailDto> GetEnrollmentDetailsAsync(Guid enrollmentId, Guid userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<EnrollmentResponseDto>> GetUserEnrollmentsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<EnrollmentResponseDto>> GetCourseEnrollmentsAsync(Guid courseId, CancellationToken cancellationToken = default);

    Task<bool> UpdateEnrollmentStatusAsync(Guid enrollmentId, EnrollmentStatus status, CancellationToken cancellationToken = default);
}

public interface IContentProgressService
{
    Task<ContentProgressDto> UpdateProgressAsync(Guid enrollmentId, UpdateProgressDto updateDto, CancellationToken cancellationToken = default);

    Task<ContentProgressDto> MarkCompletedAsync(Guid enrollmentId, Guid contentId, ContentType contentType, CancellationToken cancellationToken = default);

    Task<IEnumerable<ContentProgressDto>> GetProgressForEnrollmentAsync(Guid enrollmentId, CancellationToken cancellationToken = default);

    Task RecalculateEnrollmentProgressAsync(Guid enrollmentId, CancellationToken cancellationToken = default);
}
