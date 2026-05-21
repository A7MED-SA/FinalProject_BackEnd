using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend_project.DTOs.ContentProgress;
using backend_project.Models;

namespace backend_project.Services.Interfaces;

public interface IContentProgressService
{
    Task<ContentProgressDto> GetProgressAsync(Guid enrollmentId, ContentType contentType, Guid contentId);
    Task<IEnumerable<ContentProgressDto>> GetAllProgressForEnrollmentAsync(Guid enrollmentId);
    Task<ContentProgressDto> UpdateProgressAsync(Guid enrollmentId, ContentType contentType, Guid contentId, UpdateProgressDto updateDto);
    Task<decimal> CalculateOverallCourseProgressAsync(Guid enrollmentId);
}
