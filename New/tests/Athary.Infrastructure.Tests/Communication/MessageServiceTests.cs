using Athary.Application.DTOs.Communication;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Hubs;
using Athary.Infrastructure.Repositories;
using Athary.Infrastructure.Services.Communication;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Athary.Infrastructure.Tests.Communication;

public sealed class MessageServiceTests : SqliteTestBase
{
    private readonly MessageService _sut;
    private readonly User _student1;
    private readonly User _student2;
    private readonly User _instructor;
    private readonly Role _instructorRole;
    private readonly Role _studentRole;
    private readonly Mock<IHubContext<MessageHub>> _hubMock;

    public MessageServiceTests()
    {
        _instructor = new User { FirstName = "Inst", LastName = "R", Email = "i@r.com", UserName = "ir" };
        _student1 = new User { FirstName = "Stu1", LastName = "D", Email = "s1@d.com", UserName = "sd1" };
        _student2 = new User { FirstName = "Stu2", LastName = "E", Email = "s2@e.com", UserName = "sd2" };
        Context.Users.AddRange(_instructor, _student1, _student2);

        _instructorRole = new Role("Instructor");
        _studentRole = new Role("Student");
        Context.Roles.AddRange(_instructorRole, _studentRole);
        Context.SaveChanges();

        Context.UserRoles.AddRange(
            new UserRole { UserId = _instructor.Id, RoleId = _instructorRole.Id },
            new UserRole { UserId = _student1.Id, RoleId = _studentRole.Id },
            new UserRole { UserId = _student2.Id, RoleId = _studentRole.Id }
        );

        var cat = new Category { Name = "Cat", Slug = "cat" };
        Context.Categories.Add(cat);
        Context.SaveChanges();

        var course = new Course
        {
            Title = "C", Slug = "c", Price = 10, CategoryId = cat.Id,
            CreatedBy = _instructor.Id, Status = CourseStatus.Published, IsPublished = true
        };
        Context.Courses.Add(course);

        Context.Enrollments.Add(new Enrollment
        {
            UserId = _student1.Id, CourseId = course.Id, Status = EnrollmentStatus.InProgress
        });
        Context.SaveChanges();

        var messageRepo = new GenericRepository<Message>(Context);
        var uow = new UnitOfWork(Context);

        var roleStore = new Mock<IRoleStore<Role>>();
        roleStore.Setup(x => x.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string name, CancellationToken _) =>
            {
                var normalized = name.ToUpperInvariant();
                if (normalized == "INSTRUCTOR") return _instructorRole;
                if (normalized == "STUDENT") return _studentRole;
                return (Role?)null;
            });

        var roleManager = new RoleManager<Role>(
            roleStore.Object,
            new IRoleValidator<Role>[] { new RoleValidator<Role>() },
            new UpperInvariantLookupNormalizer(),
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<ILogger<RoleManager<Role>>>().Object);

        _hubMock = new Mock<IHubContext<MessageHub>>();
        var hubClientsMock = new Mock<IHubClients>();
        var clientProxyMock = new Mock<IClientProxy>();
        hubClientsMock.Setup(c => c.User(It.IsAny<string>())).Returns(clientProxyMock.Object);
        _hubMock.Setup(h => h.Clients).Returns(hubClientsMock.Object);

        _sut = new MessageService(messageRepo, uow, Context, _hubMock.Object, roleManager);
    }

    [Fact]
    public async Task SendMessageAsync_ShouldSend_WhenInstructorToStudent()
    {
        var result = await _sut.SendMessageAsync(_instructor.Id, new SendMessageRequest
        {
            ReceiverId = _student1.Id,
            Content = "Hello student"
        });

        result.Content.Should().Be("Hello student");
        result.SenderId.Should().Be(_instructor.Id);
        result.ReceiverId.Should().Be(_student1.Id);
        _hubMock.Verify(h => h.Clients, Times.AtLeastOnce);
    }

    [Fact]
    public async Task SendMessageAsync_ShouldSend_WhenStudentToInstructor()
    {
        var result = await _sut.SendMessageAsync(_student1.Id, new SendMessageRequest
        {
            ReceiverId = _instructor.Id,
            Content = "Hello instructor"
        });

        result.Content.Should().Be("Hello instructor");
    }

    [Fact]
    public async Task SendMessageAsync_ShouldThrow_WhenSendingToSelf()
    {
        await FluentActions.Invoking(() => _sut.SendMessageAsync(_student1.Id, new SendMessageRequest
        {
            ReceiverId = _student1.Id,
            Content = "To myself"
        })).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SendMessageAsync_ShouldThrow_WhenContentTooLong()
    {
        await FluentActions.Invoking(() => _sut.SendMessageAsync(_student1.Id, new SendMessageRequest
        {
            ReceiverId = _instructor.Id,
            Content = new string('x', 5001)
        })).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SendMessageAsync_ShouldThrow_WhenNoRelationship()
    {
        await FluentActions.Invoking(() => _sut.SendMessageAsync(_student1.Id, new SendMessageRequest
        {
            ReceiverId = _student2.Id,
            Content = "Hello stranger"
        })).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SendMessageAsync_ShouldNotCheckRelationshipAgain_WhenConversationExists()
    {
        await _sut.SendMessageAsync(_instructor.Id, new SendMessageRequest
        {
            ReceiverId = _student1.Id,
            Content = "First message"
        });

        var second = await _sut.SendMessageAsync(_student1.Id, new SendMessageRequest
        {
            ReceiverId = _instructor.Id,
            Content = "Reply"
        });

        second.Content.Should().Be("Reply");
    }

    [Fact]
    public async Task GetConversationsAsync_ShouldReturnConversations()
    {
        await _sut.SendMessageAsync(_instructor.Id, new SendMessageRequest
        {
            ReceiverId = _student1.Id,
            Content = "Hello"
        });

        var result = await _sut.GetConversationsAsync(_student1.Id);

        result.Items.Should().HaveCount(1);
        result.Items[0].OtherUserId.Should().Be(_instructor.Id);
        result.Items[0].LastMessage.Should().Be("Hello");
    }

    [Fact]
    public async Task GetConversationMessagesAsync_ShouldReturnMessages()
    {
        await _sut.SendMessageAsync(_instructor.Id, new SendMessageRequest
        {
            ReceiverId = _student1.Id,
            Content = "Hello"
        });
        await _sut.SendMessageAsync(_student1.Id, new SendMessageRequest
        {
            ReceiverId = _instructor.Id,
            Content = "Hi"
        });

        var result = await _sut.GetConversationMessagesAsync(_instructor.Id, _student1.Id);

        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ShouldReturnCorrectCount()
    {
        await _sut.SendMessageAsync(_instructor.Id, new SendMessageRequest
        {
            ReceiverId = _student1.Id,
            Content = "Unread"
        });

        var result = await _sut.GetUnreadCountAsync(_student1.Id);

        result.UnreadCount.Should().Be(1);
    }

    [Fact]
    public async Task MarkAsReadAsync_ShouldMarkMessageAsRead()
    {
        var sent = await _sut.SendMessageAsync(_instructor.Id, new SendMessageRequest
        {
            ReceiverId = _student1.Id,
            Content = "Read me"
        });

        await _sut.MarkAsReadAsync(sent.Id, _student1.Id);

        var messages = await _sut.GetConversationMessagesAsync(_instructor.Id, _student1.Id);
        messages.Items[0].IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsReadAsync_ShouldThrow_WhenNotReceiver()
    {
        var sent = await _sut.SendMessageAsync(_instructor.Id, new SendMessageRequest
        {
            ReceiverId = _student1.Id,
            Content = "Secret"
        });

        await FluentActions.Invoking(() => _sut.MarkAsReadAsync(sent.Id, _instructor.Id))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task MarkAsReadAsync_ShouldThrow_WhenNotFound()
    {
        await FluentActions.Invoking(() => _sut.MarkAsReadAsync(Guid.NewGuid(), _student1.Id))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteMessageAsync_ShouldMarkAsDeleted()
    {
        var sent = await _sut.SendMessageAsync(_instructor.Id, new SendMessageRequest
        {
            ReceiverId = _student1.Id,
            Content = "Delete me"
        });

        await _sut.DeleteMessageAsync(sent.Id, _instructor.Id);

        var msg = await Context.Messages.FindAsync(sent.Id);
        msg!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteMessageAsync_ShouldThrow_WhenNotSender()
    {
        var sent = await _sut.SendMessageAsync(_instructor.Id, new SendMessageRequest
        {
            ReceiverId = _student1.Id,
            Content = "Not yours"
        });

        await FluentActions.Invoking(() => _sut.DeleteMessageAsync(sent.Id, _student1.Id))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteMessageAsync_ShouldThrow_WhenNotFound()
    {
        await FluentActions.Invoking(() => _sut.DeleteMessageAsync(Guid.NewGuid(), _instructor.Id))
            .Should().ThrowAsync<KeyNotFoundException>();
    }
}
