using System;
using System.Threading.Tasks;
using backend_project.DTOs.Quiz;

namespace backend_project.Services.Interfaces;

public interface IQuizManagementService
{
    Task<QuizResponseDto> GetQuizAsync(Guid id);
    Task<QuizResponseDto> CreateQuizAsync(CreateQuizDto createDto);
    Task<QuizResponseDto> UpdateQuizAsync(Guid id, CreateQuizDto updateDto); // Reusing CreateQuizDto for simplicity or create UpdateQuizDto
    Task<bool> DeleteQuizAsync(Guid id); // Handles SectionItem removal

    Task<QuestionResponseDto> AddQuestionAsync(Guid quizId, CreateQuestionDto createDto);
    Task<QuestionResponseDto> UpdateQuestionAsync(Guid questionId, CreateQuestionDto updateDto);
    Task<bool> DeleteQuestionAsync(Guid questionId);
}
