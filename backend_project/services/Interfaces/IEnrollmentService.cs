using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend_project.DTOs.Enrollment;

namespace backend_project.Services.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentResponseDto> EnrollUserAsync(CreateEnrollmentDto createDto);
    Task<EnrollmentDetailDto> GetEnrollmentDetailsAsync(Guid enrollmentId, Guid userId);
    Task<IEnumerable<EnrollmentResponseDto>> GetUserEnrollmentsAsync(Guid userId);
    Task<IEnumerable<EnrollmentResponseDto>> GetCourseEnrollmentsAsync(Guid courseId);
    Task<bool> UpdateEnrollmentStatusAsync(Guid enrollmentId, Models.EnrollmentStatus status);
}
