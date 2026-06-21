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

public sealed class MessagesControllerTests
{
    private readonly Mock<IMessageService> _serviceMock;
    private readonly MessagesController _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public MessagesControllerTests()
    {
        _serviceMock = new Mock<IMessageService>();
        _sut = new MessagesController(_serviceMock.Object);

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, _userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task SendMessage_ShouldReturnOk()
    {
        var request = new SendMessageRequest { ReceiverId = Guid.NewGuid(), Content = "Hello" };
        var response = new MessageResponse { Id = Guid.NewGuid(), Content = "Hello" };
        _serviceMock.Setup(s => s.SendMessageAsync(_userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _sut.SendMessage(request, default);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        var apiResponse = ok!.Value as ApiResponse<MessageResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().Be(response);
    }

    [Fact]
    public async Task GetConversations_ShouldReturnOk()
    {
        var response = new ConversationListResponse();
        _serviceMock.Setup(s => s.GetConversationsAsync(_userId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _sut.GetConversations(1, 20, default);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        var apiResponse = ok!.Value as ApiResponse<ConversationListResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().Be(response);
    }

    [Fact]
    public async Task GetConversationMessages_ShouldReturnOk()
    {
        var otherUserId = Guid.NewGuid();
        var response = new ConversationMessagesResponse();
        _serviceMock.Setup(s => s.GetConversationMessagesAsync(_userId, otherUserId, 1, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _sut.GetConversationMessages(otherUserId, 1, 50, default);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        var apiResponse = ok!.Value as ApiResponse<ConversationMessagesResponse>;
        apiResponse.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUnreadCount_ShouldReturnOk()
    {
        var response = new UnreadCountResponse { UnreadCount = 3 };
        _serviceMock.Setup(s => s.GetUnreadCountAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _sut.GetUnreadCount(default(CancellationToken));

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        var apiResponse = ok!.Value as ApiResponse<UnreadCountResponse>;
        apiResponse!.Data!.UnreadCount.Should().Be(3);
    }

    [Fact]
    public async Task MarkAsRead_ShouldReturnOk()
    {
        var messageId = Guid.NewGuid();

        var result = await _sut.MarkAsRead(messageId, default);

        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();
        _serviceMock.Verify(s => s.MarkAsReadAsync(messageId, _userId, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task DeleteMessage_ShouldReturnNoContent()
    {
        var messageId = Guid.NewGuid();

        var result = await _sut.DeleteMessage(messageId, default);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.DeleteMessageAsync(messageId, _userId, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task SendMessage_ShouldThrow_WhenUserNotAuthenticated()
    {
        var unauthenticatedSut = new MessagesController(_serviceMock.Object);
        unauthenticatedSut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        await FluentActions.Invoking(() =>
            unauthenticatedSut.SendMessage(new SendMessageRequest(), default))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
