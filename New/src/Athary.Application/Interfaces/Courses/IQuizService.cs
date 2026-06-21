using Athary.Application.DTOs.Courses;

namespace Athary.Application.Interfaces.Courses;

public interface IQuizService
{
    Task<QuizResponseDto> GetQuizAsync(Guid id, CancellationToken cancellationToken = default);

    Task<QuizResponseDto> CreateQuizAsync(Guid courseId, CreateQuizDto createDto, CancellationToken cancellationToken = default);

    Task<QuizResponseDto> UpdateQuizAsync(Guid id, UpdateQuizDto updateDto, CancellationToken cancellationToken = default);

    Task<bool> DeleteQuizAsync(Guid id, CancellationToken cancellationToken = default);

    Task<QuestionResponseDto> AddQuestionAsync(Guid quizId, CreateQuestionDto createDto, CancellationToken cancellationToken = default);

    Task<QuestionResponseDto> UpdateQuestionAsync(Guid questionId, CreateQuestionDto updateDto, CancellationToken cancellationToken = default);

    Task<bool> DeleteQuestionAsync(Guid questionId, CancellationToken cancellationToken = default);
}
