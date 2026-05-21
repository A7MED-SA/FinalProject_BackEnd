using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend_project.DTOs.ContentProgress;
using backend_project.Models;

namespace backend_project.Services.Interfaces;

public interface IContentProgressService
{
    Task<ContentProgressDto> UpdateProgressAsync(Guid enrollmentId, UpdateProgressDto updateDto);
    Task<ContentProgressDto> MarkCompletedAsync(Guid enrollmentId, Guid contentId, ContentType contentType);
    Task<IEnumerable<ContentProgressDto>> GetProgressForEnrollmentAsync(Guid enrollmentId);
    Task RecalculateEnrollmentProgressAsync(Guid enrollmentId);
}
