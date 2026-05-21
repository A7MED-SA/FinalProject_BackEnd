using System;
using backend_project.Models;

namespace backend_project.DTOs.Enrollment;

public class CreateEnrollmentDto
{
    public Guid CourseId { get; set; }
    public Guid UserId { get; set; }
    public EnrollmentSource Source { get; set; } = EnrollmentSource.Purchase;
}
