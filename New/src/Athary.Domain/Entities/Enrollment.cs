using Athary.Domain.Enums;

namespace Athary.Domain.Entities;

public sealed class Enrollment : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.InProgress;
    public DateTime? CompletedAt { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public decimal ProgressPercentage { get; set; }
    public Guid? CertificateId { get; set; }
    public EnrollmentSource Source { get; set; } = EnrollmentSource.Purchase;
    public DateTime? AccessExpiresAt { get; set; }
    public bool IsRefunded { get; set; }

    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public Certificate? Certificate { get; set; }
    public List<ContentProgress> ContentProgresses { get; set; } = new List<ContentProgress>();
    public List<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
}
