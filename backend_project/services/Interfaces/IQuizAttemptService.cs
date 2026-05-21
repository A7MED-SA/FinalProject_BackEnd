using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend_project.DTOs.QuizAttempt;

namespace backend_project.Services.Interfaces;

public interface IQuizAttemptService
{
    Task<QuizAttemptResponseDto> StartAttemptAsync(Guid quizId, Guid enrollmentId);
    Task<QuizResultDto> SubmitAttemptAsync(Guid attemptId, SubmitAttemptDto submitDto);
    Task<QuizResultDto> GetAttemptResultAsync(Guid attemptId);
    Task<IEnumerable<QuizAttemptResponseDto>> GetUserAttemptsForQuizAsync(Guid quizId, Guid enrollmentId);
}
