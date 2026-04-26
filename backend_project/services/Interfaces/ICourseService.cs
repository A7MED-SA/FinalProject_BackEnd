using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend_project.DTOs.Course;

namespace backend_project.Services.Interfaces;

public interface ICourseService
{
    // US1
    Task<CourseDetailsDto> CreateCourseAsync(Guid instructorId, CreateCourseDto dto);
    Task<CourseDetailsDto> UpdateCourseAsync(Guid courseId, Guid instructorId, UpdateCourseDto dto);
    Task<CourseDetailsDto> GetCourseByIdAsync(Guid courseId);
    
    Task<CourseRequirementDto> AddRequirementAsync(Guid courseId, Guid instructorId, AddRequirementDto dto);
    Task RemoveRequirementAsync(Guid courseId, Guid requirementId, Guid instructorId);

    Task<CourseLearningOutcomeDto> AddLearningOutcomeAsync(Guid courseId, Guid instructorId, AddLearningOutcomeDto dto);
    Task RemoveLearningOutcomeAsync(Guid courseId, Guid outcomeId, Guid instructorId);
    
    Task SubmitForReviewAsync(Guid courseId, Guid instructorId);

    // US4 - Admin Approval
    Task<List<CourseSummaryDto>> GetPendingCoursesAsync();
    Task ApproveCourseAsync(Guid courseId, Guid adminId);
    Task RejectCourseAsync(Guid courseId, Guid adminId, string reason);
    Task<CourseDetailsDto> SetCourseImageAsync(Guid courseId, Guid fileId, Guid userId);
    Task<CourseDetailsDto> RemoveCourseImageAsync(Guid courseId, Guid userId);
}
