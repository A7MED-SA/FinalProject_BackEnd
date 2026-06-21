using System.Security.Claims;
using Athary.Application.Common;
using Athary.Application.DTOs.Certificate;
using Athary.Application.Interfaces.Certificate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/certificates")]
public class CertificatesController : ControllerBase
{
    private readonly ICertificateService _certificateService;

    public CertificatesController(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<CertificateResponse>>>> GetMyCertificates(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var certificates = await _certificateService.GetMyCertificatesAsync(userId, cancellationToken);
        return Ok(ApiResponse<List<CertificateResponse>>.SuccessResponse(certificates));
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CertificateResponse>>> GetCertificate(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var certificate = await _certificateService.GetCertificateByIdAsync(id, userId, cancellationToken);
        return Ok(ApiResponse<CertificateResponse>.SuccessResponse(certificate));
    }

    [HttpGet("{id:guid}/download")]
    [Authorize]
    public async Task<IActionResult> DownloadCertificate(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var stream = await _certificateService.GetCertificatePdfStreamAsync(id, userId, cancellationToken);
        return File(stream, "application/pdf", $"certificate-{id}.pdf");
    }

    [HttpGet("verify/{code}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<CertificateVerificationResponse>>> VerifyCertificate(string code, CancellationToken cancellationToken)
    {
        var result = await _certificateService.VerifyCertificateAsync(code, cancellationToken);
        return Ok(ApiResponse<CertificateVerificationResponse>.SuccessResponse(result));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(userIdClaim);
    }
}
