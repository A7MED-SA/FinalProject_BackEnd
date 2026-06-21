using Athary.Application.DTOs.InstructorRequests.Requests;
using Athary.Application.Interfaces.Authentication;
using Athary.Application.Interfaces.InstructorRequests;
using Athary.Application.Interfaces.Notification;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Services.InstructorRequests;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Athary.Infrastructure.Tests.InstructorRequests;

public class InstructorRequestServiceTests : SqliteTestBase
{
    private readonly IInstructorRequestService _sut;
    private readonly User _user;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;

    public InstructorRequestServiceTests()
    {
        _user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            UserName = "testuser"
        };
        Context.Users.Add(_user);
        Context.SaveChanges();

        _userManagerMock = CreateUserManagerMock();
        _emailServiceMock = new Mock<IEmailService>();
        _notificationServiceMock = new Mock<INotificationService>();

        _sut = new InstructorRequestService(
            Context,
            _userManagerMock.Object,
            _emailServiceMock.Object,
            _notificationServiceMock.Object);
    }

    [Fact]
    public async Task CanSubmitRequestAsync_ShouldReturnTrue_WhenNoPendingRequestAndNotTeacher()
    {
        _userManagerMock.Setup(x => x.FindByIdAsync(_user.Id.ToString()))
            .ReturnsAsync(_user);
        _userManagerMock.Setup(x => x.IsInRoleAsync(_user, "Instructor"))
            .ReturnsAsync(false);

        var result = await _sut.CanSubmitRequestAsync(_user.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanSubmitRequestAsync_ShouldReturnFalse_WhenHasPendingRequest()
    {
        Context.InstructorRequests.Add(new InstructorRequest
        {
            UserId = _user.Id,
            Status = InstructorRequestStatus.Pending
        });
        Context.SaveChanges();

        _userManagerMock.Setup(x => x.FindByIdAsync(_user.Id.ToString()))
            .ReturnsAsync(_user);
        _userManagerMock.Setup(x => x.IsInRoleAsync(_user, "Instructor"))
            .ReturnsAsync(false);

        var result = await _sut.CanSubmitRequestAsync(_user.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanSubmitRequestAsync_ShouldReturnFalse_WhenAlreadyTeacher()
    {
        _userManagerMock.Setup(x => x.FindByIdAsync(_user.Id.ToString()))
            .ReturnsAsync(_user);
        _userManagerMock.Setup(x => x.IsInRoleAsync(_user, "Instructor"))
            .ReturnsAsync(true);

        var result = await _sut.CanSubmitRequestAsync(_user.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SubmitRequestAsync_ShouldCreateRequest_WhenValid()
    {
        _userManagerMock.Setup(x => x.FindByIdAsync(_user.Id.ToString()))
            .ReturnsAsync(_user);
        _userManagerMock.Setup(x => x.IsInRoleAsync(_user, "Instructor"))
            .ReturnsAsync(false);

        var dto = new SubmitInstructorRequestDto
        {
            Message = "I want to be an instructor",
            Documents = new List<InstructorRequestDocumentDto>
            {
                new() { DocumentType = DocumentType.CV, FileId = Guid.NewGuid() }
            }
        };

        var result = await _sut.SubmitRequestAsync(_user.Id, dto);

        result.Should().NotBeNull();
        result.Message.Should().Be("I want to be an instructor");
        result.Status.Should().Be(InstructorRequestStatus.Pending);

        var saved = await Context.InstructorRequests
            .Include(r => r.Documents)
            .FirstOrDefaultAsync(r => r.UserId == _user.Id);
        saved.Should().NotBeNull();
        saved!.Documents.Should().HaveCount(1);

        _notificationServiceMock.Verify(x => x.CreateAndSendNotificationAsync(
            _user.Id,
            It.IsAny<string>(),
            It.IsAny<string>(),
            NotificationType.InstructorRequest,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitRequestAsync_ShouldThrow_WhenCannotSubmit()
    {
        _userManagerMock.Setup(x => x.FindByIdAsync(_user.Id.ToString()))
            .ReturnsAsync(_user);
        _userManagerMock.Setup(x => x.IsInRoleAsync(_user, "Instructor"))
            .ReturnsAsync(true);

        var dto = new SubmitInstructorRequestDto
        {
            Message = "Test",
            Documents = new List<InstructorRequestDocumentDto>
            {
                new() { DocumentType = DocumentType.CV, FileId = Guid.NewGuid() }
            }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SubmitRequestAsync(_user.Id, dto));
    }

    [Fact]
    public async Task GetMyRequestsAsync_ShouldReturnUserRequests()
    {
        Context.InstructorRequests.Add(new InstructorRequest
        {
            UserId = _user.Id,
            Message = "Request 1",
            Status = InstructorRequestStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        });
        Context.InstructorRequests.Add(new InstructorRequest
        {
            UserId = _user.Id,
            Message = "Request 2",
            Status = InstructorRequestStatus.Approved,
            SubmittedAt = DateTime.UtcNow.AddMinutes(-10)
        });
        Context.SaveChanges();

        var results = await _sut.GetMyRequestsAsync(_user.Id);

        results.Should().HaveCount(2);
        results.First().Message.Should().Be("Request 1"); // Ordered by SubmittedAt desc
    }

    [Fact]
    public async Task GetMyRequestByIdAsync_ShouldReturnDetail_WhenOwnRequest()
    {
        var request = new InstructorRequest
        {
            UserId = _user.Id,
            Message = "Detail test",
            Status = InstructorRequestStatus.Pending
        };
        Context.InstructorRequests.Add(request);
        Context.SaveChanges();

        var result = await _sut.GetMyRequestByIdAsync(_user.Id, request.Id);

        result.Should().NotBeNull();
        result.Message.Should().Be("Detail test");
    }

    [Fact]
    public async Task GetMyRequestByIdAsync_ShouldThrow_WhenNotOwnRequest()
    {
        var otherUser = new User { Id = Guid.NewGuid(), FirstName = "Other", LastName = "User", Email = "other@test.com", UserName = "other" };
        Context.Users.Add(otherUser);
        var request = new InstructorRequest { UserId = otherUser.Id, Message = "Not mine", Status = InstructorRequestStatus.Pending };
        Context.InstructorRequests.Add(request);
        Context.SaveChanges();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetMyRequestByIdAsync(_user.Id, request.Id));
    }

    [Fact]
    public async Task CancelRequestAsync_ShouldCancelPendingRequest()
    {
        var request = new InstructorRequest
        {
            UserId = _user.Id,
            Message = "Cancel me",
            Status = InstructorRequestStatus.Pending
        };
        Context.InstructorRequests.Add(request);
        Context.SaveChanges();

        var result = await _sut.CancelRequestAsync(_user.Id, request.Id);

        result.Should().BeTrue();

        var saved = await Context.InstructorRequests.FindAsync(request.Id);
        saved!.Status.Should().Be(InstructorRequestStatus.Rejected);
        saved.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CancelRequestAsync_ShouldReturnFalse_WhenNotPending()
    {
        var request = new InstructorRequest
        {
            UserId = _user.Id,
            Message = "Already processed",
            Status = InstructorRequestStatus.Approved
        };
        Context.InstructorRequests.Add(request);
        Context.SaveChanges();

        var result = await _sut.CancelRequestAsync(_user.Id, request.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteRequestAsync_ShouldRemoveRequest()
    {
        var request = new InstructorRequest
        {
            UserId = _user.Id,
            Message = "Delete me",
            Status = InstructorRequestStatus.Pending
        };
        Context.InstructorRequests.Add(request);
        Context.SaveChanges();

        var result = await _sut.DeleteRequestAsync(request.Id);

        result.Should().BeTrue();
        var saved = await Context.InstructorRequests.FindAsync(request.Id);
        saved.Should().BeNull();
    }

    [Fact]
    public async Task DeleteRequestAsync_ShouldReturnFalse_WhenNotFound()
    {
        var result = await _sut.DeleteRequestAsync(Guid.NewGuid());
        result.Should().BeFalse();
    }

    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        var store = Mock.Of<IUserStore<User>>();
        return new Mock<UserManager<User>>(store, null!, null!, null!, null!, null!, null!, null!, null!);
    }
}
