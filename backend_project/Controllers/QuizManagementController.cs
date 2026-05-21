using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.DTOs.Quiz;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/courses/{courseId}/quizzes")]
[Authorize(Roles = "Instructor")]
public class QuizManagementController : ControllerBase
{
    private readonly IQuizManagementService _quizManagementService;

    public QuizManagementController(IQuizManagementService quizManagementService)
    {
        _quizManagementService = quizManagementService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetQuiz(Guid courseId, Guid id)
    {
        try
        {
            var result = await _quizManagementService.GetQuizAsync(id);
            return Ok(ApiResponse<QuizResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuizResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateQuiz(Guid courseId, [FromBody] CreateQuizDto createDto)
    {
        try
        {
            var result = await _quizManagementService.CreateQuizAsync(createDto);
            return CreatedAtAction(nameof(GetQuiz), new { courseId, id = result.Id }, ApiResponse<QuizResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuizResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateQuiz(Guid courseId, Guid id, [FromBody] CreateQuizDto updateDto)
    {
        try
        {
            var result = await _quizManagementService.UpdateQuizAsync(id, updateDto);
            return Ok(ApiResponse<QuizResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuizResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuiz(Guid courseId, Guid id)
    {
        try
        {
            var result = await _quizManagementService.DeleteQuizAsync(id);
            if (!result)
                return NotFound(ApiResponse<object>.FailureResponse("Quiz not found."));

            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Deleted" }));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("{quizId}/questions")]
    public async Task<IActionResult> AddQuestion(Guid courseId, Guid quizId, [FromBody] CreateQuestionDto createDto)
    {
        try
        {
            var result = await _quizManagementService.AddQuestionAsync(quizId, createDto);
            return CreatedAtAction(nameof(GetQuiz), new { courseId, id = quizId }, ApiResponse<QuestionResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuestionResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{quizId}/questions/{questionId}")]
    public async Task<IActionResult> UpdateQuestion(Guid courseId, Guid quizId, Guid questionId, [FromBody] CreateQuestionDto updateDto)
    {
        try
        {
            var result = await _quizManagementService.UpdateQuestionAsync(questionId, updateDto);
            return Ok(ApiResponse<QuestionResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuestionResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{quizId}/questions/{questionId}")]
    public async Task<IActionResult> DeleteQuestion(Guid courseId, Guid quizId, Guid questionId)
    {
        var result = await _quizManagementService.DeleteQuestionAsync(questionId);
        if (!result)
            return NotFound(ApiResponse<object>.FailureResponse("Question not found."));

        return Ok(ApiResponse<object>.SuccessResponse(new { message = "Deleted" }));
    }

    private Guid GetUserId()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        return userId;
    }
}
