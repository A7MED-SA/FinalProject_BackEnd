using Athary.Application.Common;
using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers.Courses;

[ApiController]
[Route("api/courses/{courseId:guid}/quizzes")]
[Authorize(Roles = "Instructor")]
public class QuizManagementController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizManagementController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<QuizResponseDto>>> GetQuiz(
        Guid courseId,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _quizService.GetQuizAsync(id, cancellationToken);
            return Ok(ApiResponse<QuizResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuizResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<QuizResponseDto>>> CreateQuiz(
        Guid courseId,
        [FromBody] CreateQuizDto createDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _quizService.CreateQuizAsync(courseId, createDto, cancellationToken);
            return CreatedAtAction(nameof(GetQuiz), new { courseId, id = result.Id }, ApiResponse<QuizResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuizResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<QuizResponseDto>>> UpdateQuiz(
        Guid courseId,
        Guid id,
        [FromBody] UpdateQuizDto updateDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _quizService.UpdateQuizAsync(id, updateDto, cancellationToken);
            return Ok(ApiResponse<QuizResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuizResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteQuiz(
        Guid courseId,
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _quizService.DeleteQuizAsync(id, cancellationToken);
            if (!result)
                return NotFound(ApiResponse<object>.FailureResponse("Quiz not found."));

            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Deleted" }));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("{quizId:guid}/questions")]
    public async Task<ActionResult<ApiResponse<QuestionResponseDto>>> AddQuestion(
        Guid courseId,
        Guid quizId,
        [FromBody] CreateQuestionDto createDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _quizService.AddQuestionAsync(quizId, createDto, cancellationToken);
            return CreatedAtAction(nameof(GetQuiz), new { courseId, id = quizId }, ApiResponse<QuestionResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuestionResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{quizId:guid}/questions/{questionId:guid}")]
    public async Task<ActionResult<ApiResponse<QuestionResponseDto>>> UpdateQuestion(
        Guid courseId,
        Guid quizId,
        Guid questionId,
        [FromBody] CreateQuestionDto updateDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _quizService.UpdateQuestionAsync(questionId, updateDto, cancellationToken);
            return Ok(ApiResponse<QuestionResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuestionResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("{quizId:guid}/questions/{questionId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteQuestion(
        Guid courseId,
        Guid quizId,
        Guid questionId,
        CancellationToken cancellationToken)
    {
        var result = await _quizService.DeleteQuestionAsync(questionId, cancellationToken);
        if (!result)
            return NotFound(ApiResponse<object>.FailureResponse("Question not found."));

        return Ok(ApiResponse<object>.SuccessResponse(new { message = "Deleted" }));
    }
}
