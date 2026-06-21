using System.Security.Claims;
using Athary.Application.Common;
using Athary.Application.DTOs.Certificate;
using Athary.Application.Interfaces.Certificate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Athary.API.Controllers;

[ApiController]
[Route("api/admin/certificates")]
[Authorize(Roles = "Admin")]
public class AdminCertificatesController : ControllerBase
{
    private readonly ICertificateService _certificateService;

    public AdminCertificatesController(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CertificateResponse>>>> GetAllCertificates(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var certificates = await _certificateService.GetAllCertificatesAsync(page, pageSize, cancellationToken);
        return Ok(ApiResponse<List<CertificateResponse>>.SuccessResponse(certificates));
    }

    [HttpPost("{id:guid}/revoke")]
    public async Task<ActionResult<ApiResponse<object>>> RevokeCertificate(Guid id, [FromBody] RevokeCertificateRequest? request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _certificateService.RevokeCertificateAsync(id, userId, request?.Reason, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Certificate revoked successfully."));
    }

    [HttpPost("issue")]
    public async Task<ActionResult<ApiResponse<CertificateResponse>>> IssueCertificate([FromBody] IssueCertificateRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _certificateService.IssueCertificateManuallyAsync(userId, request, cancellationToken);
        return Ok(ApiResponse<CertificateResponse>.SuccessResponse(result));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(userIdClaim);
    }
}
