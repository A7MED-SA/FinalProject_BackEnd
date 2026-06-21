using Athary.Application.DTOs.Courses;

namespace Athary.Application.Interfaces.Courses;

public interface IQuizAttemptService
{
    Task<QuizAttemptResponseDto> StartAttemptAsync(Guid quizId, Guid enrollmentId, CancellationToken cancellationToken = default);

    Task<QuizResultDto> SubmitAttemptAsync(Guid attemptId, SubmitAttemptDto submitDto, CancellationToken cancellationToken = default);

    Task<QuizResultDto> GetAttemptResultAsync(Guid attemptId, CancellationToken cancellationToken = default);

    Task<IEnumerable<QuizAttemptResponseDto>> GetUserAttemptsForQuizAsync(Guid quizId, Guid enrollmentId, CancellationToken cancellationToken = default);
}
