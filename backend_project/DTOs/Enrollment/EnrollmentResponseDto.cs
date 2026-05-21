using System;
using backend_project.Models;

namespace backend_project.DTOs.Enrollment;

public class EnrollmentResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public EnrollmentStatus Status { get; set; }
    public decimal ProgressPercentage { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public EnrollmentSource Source { get; set; }
    public DateTime? AccessExpiresAt { get; set; }
    public bool IsRefunded { get; set; }
}
