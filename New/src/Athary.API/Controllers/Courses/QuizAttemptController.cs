using Athary.Application.Common;
using Athary.Application.DTOs.Courses;
using Athary.Application.Interfaces.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers.Courses;

[ApiController]
[Route("api/enrollments/{enrollmentId:guid}/quizzes/{quizId:guid}/attempts")]
[Authorize]
public class QuizAttemptController : ControllerBase
{
    private readonly IQuizAttemptService _quizAttemptService;

    public QuizAttemptController(IQuizAttemptService quizAttemptService)
    {
        _quizAttemptService = quizAttemptService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<QuizAttemptResponseDto>>> StartAttempt(
        Guid enrollmentId,
        Guid quizId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _quizAttemptService.StartAttemptAsync(quizId, enrollmentId, cancellationToken);
            return CreatedAtAction(nameof(GetAttempts), new { enrollmentId, quizId }, ApiResponse<QuizAttemptResponseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuizAttemptResponseDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<QuizAttemptResponseDto>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("{attemptId:guid}")]
    public async Task<ActionResult<ApiResponse<QuizResultDto>>> SubmitAttempt(
        Guid enrollmentId,
        Guid quizId,
        Guid attemptId,
        [FromBody] SubmitAttemptDto submitDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _quizAttemptService.SubmitAttemptAsync(attemptId, submitDto, cancellationToken);
            return Ok(ApiResponse<QuizResultDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuizResultDto>.FailureResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<QuizResultDto>.FailureResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<QuizAttemptResponseDto>>>> GetAttempts(
        Guid enrollmentId,
        Guid quizId,
        CancellationToken cancellationToken)
    {
        var result = await _quizAttemptService.GetUserAttemptsForQuizAsync(quizId, enrollmentId, cancellationToken);
        return Ok(ApiResponse<IEnumerable<QuizAttemptResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{attemptId:guid}")]
    public async Task<ActionResult<ApiResponse<QuizResultDto>>> GetAttemptResult(
        Guid enrollmentId,
        Guid quizId,
        Guid attemptId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _quizAttemptService.GetAttemptResultAsync(attemptId, cancellationToken);
            return Ok(ApiResponse<QuizResultDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuizResultDto>.FailureResponse(ex.Message));
        }
    }
}
