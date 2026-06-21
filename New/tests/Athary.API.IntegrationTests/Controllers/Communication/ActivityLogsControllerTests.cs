using System.Security.Claims;
using Athary.API.Controllers.Communication;
using Athary.Application.Common;
using Athary.Application.DTOs.ActivityLog;
using Athary.Application.Interfaces.Authentication;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Athary.API.IntegrationTests.Controllers.Communication;

public sealed class ActivityLogsControllerTests
{
    private readonly Mock<IActivityLogService> _serviceMock;
    private readonly ActivityLogsController _sut;

    public ActivityLogsControllerTests()
    {
        _serviceMock = new Mock<IActivityLogService>();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);

        _sut = new ActivityLogsController(_serviceMock.Object);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetLogs_ShouldReturnOk()
    {
        var log = new ActivityLog
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Action = "Login",
            EntityType = ActivityLogEntityType.User,
            EntityId = Guid.NewGuid(),
            Details = "User logged in",
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow,
            User = new User { FirstName = "Admin", LastName = "User" }
        };
        _serviceMock.Setup(s => s.GetLogsAsync(
                It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string>(),
                1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { log } as IEnumerable<ActivityLog>, 1));

        var filter = new ActivityLogFilterRequest { Page = 1, PageSize = 20 };

        var result = await _sut.GetLogs(filter, default);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        var apiResponse = ok!.Value as ApiResponse<ActivityLogListResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data!.Items.Should().HaveCount(1);
        apiResponse.Data.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetLogs_WithFilters_ShouldPassToService()
    {
        _serviceMock.Setup(s => s.GetLogsAsync(
                It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enumerable.Empty<ActivityLog>(), 0));

        var filter = new ActivityLogFilterRequest
        {
            UserId = Guid.NewGuid(),
            Action = "Login",
            EntityType = "User",
            DateFrom = DateTime.UtcNow.AddDays(-7),
            DateTo = DateTime.UtcNow,
            IpAddress = "192.168.1.1",
            Page = 2,
            PageSize = 10
        };

        await _sut.GetLogs(filter, default);

        _serviceMock.Verify(s => s.GetLogsAsync(
            filter.UserId, filter.Action, filter.EntityType,
            filter.DateFrom, filter.DateTo, filter.IpAddress,
            filter.Page, filter.PageSize, It.IsAny<CancellationToken>()));
    }
}
