using System.Security.Claims;
using Athary.API.Controllers.Communication;
using Athary.Application.Common;
using Athary.Application.DTOs.Communication;
using Athary.Application.Interfaces.Communication;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Athary.API.IntegrationTests.Controllers.Communication;

public sealed class ReportsControllerTests
{
    private readonly Mock<IReportService> _serviceMock;
    private readonly ReportsController _sut;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _adminUserId = Guid.NewGuid();

    public ReportsControllerTests()
    {
        _serviceMock = new Mock<IReportService>();

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, _userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);
        _sut = new ReportsController(_serviceMock.Object);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task Create_ShouldReturnOk()
    {
        var request = new CreateReportRequest { EntityType = "Course", EntityId = Guid.NewGuid(), Reason = "Spam" };
        var response = new ReportResponse { Id = Guid.NewGuid(), Reason = "Spam" };
        _serviceMock.Setup(s => s.CreateAsync(_userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _sut.Create(request, default);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        var apiResponse = ok!.Value as ApiResponse<ReportResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().Be(response);
    }

    [Fact]
    public async Task GetPending_ShouldReturnOk()
    {
        var response = new ReportListResponse();
        _serviceMock.Setup(s => s.GetPendingAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var adminSut = CreateAdminController();
        var result = await adminSut.GetPending(1, 20, default);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        var apiResponse = ok!.Value as ApiResponse<ReportListResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().Be(response);
    }

    [Fact]
    public async Task Resolve_ShouldReturnOk()
    {
        var reportId = Guid.NewGuid();
        var request = new ResolveReportRequest { Status = "Resolved", AdminNote = "Warned user" };
        var response = new ReportResponse { Id = reportId, Status = "Resolved", AdminNote = "Warned user" };
        _serviceMock.Setup(s => s.ResolveAsync(reportId, request, _adminUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var adminSut = CreateAdminController();
        var result = await adminSut.Resolve(reportId, request, default);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        var apiResponse = ok!.Value as ApiResponse<ReportResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().Be(response);
    }

    private ReportsController CreateAdminController()
    {
        var adminClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, _adminUserId.ToString()),
            new(ClaimTypes.Role, "Admin")
        };
        var adminIdentity = new ClaimsIdentity(adminClaims, "test");
        var adminPrincipal = new ClaimsPrincipal(adminIdentity);
        return new ReportsController(_serviceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = adminPrincipal }
            }
        };
    }
}
