using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_project.DTOs;
using backend_project.DTOs.Certificate;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminCertificatesController : ControllerBase
{
    private readonly ICertificateService _certificateService;

    public AdminCertificatesController(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCertificates([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var certificates = await _certificateService.GetAllCertificatesAsync(page, pageSize);
        return Ok(ApiResponse<IEnumerable<CertificateResponse>>.SuccessResponse(certificates));
    }

    [HttpPost("{id}/revoke")]
    public async Task<IActionResult> RevokeCertificate(Guid id, [FromBody] RevokeCertificateRequest? request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            await _certificateService.RevokeCertificateAsync(id, userId.Value, request?.Reason);
            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Certificate revoked successfully." }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("issue")]
    public async Task<IActionResult> IssueCertificate([FromBody] IssueCertificateRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var result = await _certificateService.IssueCertificateManuallyAsync(userId.Value, request);
            return Ok(ApiResponse<CertificateResponse>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(value) || !Guid.TryParse(value, out var userId))
            return null;
        return userId;
    }
}
