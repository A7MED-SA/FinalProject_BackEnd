using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using backend_project.DTOs;
using backend_project.DTOs.Certificate;
using backend_project.Services.Interfaces;

namespace backend_project.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CertificatesController : ControllerBase
{
    private readonly ICertificateService _certificateService;

    public CertificatesController(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyCertificates()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var certificates = await _certificateService.GetMyCertificatesAsync(userId.Value);
        return Ok(ApiResponse<IEnumerable<CertificateResponse>>.SuccessResponse(certificates));
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetCertificate(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var certificate = await _certificateService.GetCertificateByIdAsync(id, userId.Value);
            return Ok(ApiResponse<CertificateResponse>.SuccessResponse(certificate));
        }
        catch (Exception ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("{id}/download")]
    [Authorize]
    public async Task<IActionResult> DownloadCertificate(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var stream = await _certificateService.GetCertificatePdfStreamAsync(id, userId.Value);
            return File(stream, "application/pdf", $"certificate-{id}.pdf");
        }
        catch (Exception ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpGet("verify/{code}")]
    [AllowAnonymous]
    [EnableRateLimiting("CertificateVerification")]
    public async Task<IActionResult> VerifyCertificate(string code)
    {
        var result = await _certificateService.VerifyCertificateAsync(code);
        return Ok(ApiResponse<CertificateVerificationResponse>.SuccessResponse(result));
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(value) || !Guid.TryParse(value, out var userId))
            return null;
        return userId;
    }
}
