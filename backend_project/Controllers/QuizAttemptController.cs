using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.DTOs.QuizAttempt;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/enrollments/{enrollmentId}/quizzes/{quizId}/attempts")]
[Authorize]
public class QuizAttemptController : ControllerBase
{
    private readonly IQuizAttemptService _quizAttemptService;

    public QuizAttemptController(IQuizAttemptService quizAttemptService)
    {
        _quizAttemptService = quizAttemptService;
    }

    [HttpPost]
    public async Task<IActionResult> StartAttempt(Guid enrollmentId, Guid quizId)
    {
        try
        {
            var result = await _quizAttemptService.StartAttemptAsync(quizId, enrollmentId);
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

    [HttpPut("{attemptId}")]
    public async Task<IActionResult> SubmitAttempt(Guid enrollmentId, Guid quizId, Guid attemptId, [FromBody] SubmitAttemptDto submitDto)
    {
        try
        {
            var result = await _quizAttemptService.SubmitAttemptAsync(attemptId, submitDto);
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
    public async Task<IActionResult> GetAttempts(Guid enrollmentId, Guid quizId)
    {
        var result = await _quizAttemptService.GetUserAttemptsForQuizAsync(quizId, enrollmentId);
        return Ok(ApiResponse<IEnumerable<QuizAttemptResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{attemptId}")]
    public async Task<IActionResult> GetAttemptResult(Guid enrollmentId, Guid quizId, Guid attemptId)
    {
        try
        {
            var result = await _quizAttemptService.GetAttemptResultAsync(attemptId);
            return Ok(ApiResponse<QuizResultDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<QuizResultDto>.FailureResponse(ex.Message));
        }
    }

    private Guid GetUserId()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        return userId;
    }
}
