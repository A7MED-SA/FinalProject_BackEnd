using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Services.Communication;
using FluentAssertions;

namespace Athary.Infrastructure.Tests.Communication;

public sealed class ActivityLogServiceTests : SqliteTestBase
{
    private readonly ActivityLogService _sut;
    private readonly User _user1;
    private readonly User _user2;

    public ActivityLogServiceTests()
    {
        _user1 = new User { FirstName = "Alice", LastName = "A", Email = "a@a.com", UserName = "aa" };
        _user2 = new User { FirstName = "Bob", LastName = "B", Email = "b@b.com", UserName = "bb" };
        Context.Users.AddRange(_user1, _user2);
        Context.SaveChanges();

        _sut = new ActivityLogService(
            new Athary.Infrastructure.Repositories.GenericRepository<ActivityLog>(Context),
            new Athary.Infrastructure.Repositories.UnitOfWork(Context),
            Context);
    }

    [Fact]
    public async Task LogActivityAsync_ShouldCreateLog()
    {
        await _sut.LogActivityAsync(_user1.Id, "Login", "User logged in", "192.168.1.1");

        var (items, total) = await _sut.GetLogsAsync();
        total.Should().Be(1);
        items.Should().ContainSingle(l =>
            l.Action == "Login" &&
            l.UserId == _user1.Id &&
            l.IpAddress == "192.168.1.1");
    }

    [Fact]
    public async Task LogActivityAsync_ShouldSetEntityTypeToUser()
    {
        await _sut.LogActivityAsync(_user1.Id, "Register", "User registered", "10.0.0.1");

        var items = (await _sut.GetLogsAsync()).Items.ToList();
        items[0].EntityType.Should().Be(ActivityLogEntityType.User);
    }

    [Fact]
    public async Task LogActivityAsync_ShouldSetCreatedAt()
    {
        await _sut.LogActivityAsync(_user1.Id, "Logout", "Logged out", "::1");

        var items = (await _sut.GetLogsAsync()).Items.ToList();
        items[0].CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task LogDeletionAsync_ShouldCreateDeletionLog()
    {
        var entityId = Guid.NewGuid();
        await _sut.LogDeletionAsync(_user1.Id, ActivityLogEntityType.Message, entityId, "Deleted message");

        var (items, total) = await _sut.GetLogsAsync();
        total.Should().Be(1);
        items.Should().ContainSingle(l =>
            l.Action == "Delete" &&
            l.EntityType == ActivityLogEntityType.Message &&
            l.EntityId == entityId);
    }

    [Fact]
    public async Task LogDeletionAsync_ShouldUseDifferentEntityTypes()
    {
        var entityId = Guid.NewGuid();
        await _sut.LogDeletionAsync(_user1.Id, ActivityLogEntityType.Review, entityId, "Deleted review");

        var items = (await _sut.GetLogsAsync()).Items.ToList();
        items[0].EntityType.Should().Be(ActivityLogEntityType.Review);
    }

    private async Task SeedOrderedLogsAsync()
    {
        await _sut.LogActivityAsync(_user1.Id, "Login", "A", "1.1.1.1");
        await _sut.LogActivityAsync(_user1.Id, "Logout", "B", "2.2.2.2");
        await _sut.LogActivityAsync(_user2.Id, "Login", "C", "3.3.3.3");
    }

    [Fact]
    public async Task GetLogsAsync_ShouldReturnAllLogs()
    {
        await SeedOrderedLogsAsync();

        var (items, total) = await _sut.GetLogsAsync();
        total.Should().Be(3);
        items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetLogsAsync_ShouldReturnInDescendingOrder()
    {
        await _sut.LogActivityAsync(_user1.Id, "First", "Earliest", "1.1.1.1");
        await Task.Delay(10);
        await _sut.LogActivityAsync(_user2.Id, "Second", "Latest", "2.2.2.2");

        var items = (await _sut.GetLogsAsync()).Items.ToList();
        items[0].Action.Should().Be("Second");
        items[1].Action.Should().Be("First");
    }

    [Fact]
    public async Task GetLogsAsync_ShouldFilterByUserId()
    {
        await SeedOrderedLogsAsync();

        var (items, total) = await _sut.GetLogsAsync(userId: _user1.Id);
        total.Should().Be(2);
        items.Should().AllSatisfy(l => l.UserId.Should().Be(_user1.Id));
    }

    [Fact]
    public async Task GetLogsAsync_ShouldFilterByAction()
    {
        await SeedOrderedLogsAsync();

        var (items, total) = await _sut.GetLogsAsync(action: "Login");
        total.Should().Be(2);
        items.Should().AllSatisfy(l => l.Action.Should().Be("Login"));
    }

    [Fact]
    public async Task GetLogsAsync_ShouldFilterByEntityType()
    {
        var entityId = Guid.NewGuid();
        await _sut.LogDeletionAsync(_user1.Id, ActivityLogEntityType.Message, entityId, "Msg deleted");
        await _sut.LogDeletionAsync(_user1.Id, ActivityLogEntityType.Review, entityId, "Review deleted");

        var (items, total) = await _sut.GetLogsAsync(entityType: "Review");
        total.Should().Be(1);
        items.Should().AllSatisfy(l => l.EntityType.Should().Be(ActivityLogEntityType.Review));
    }

    [Fact]
    public async Task GetLogsAsync_ShouldFilterByDateRange()
    {
        await _sut.LogActivityAsync(_user1.Id, "Old", "Past event", "1.1.1.1");

        var from = DateTime.UtcNow.AddHours(1);
        (await _sut.GetLogsAsync(dateFrom: from)).TotalCount.Should().Be(0);

        var to = DateTime.UtcNow.AddHours(-1);
        (await _sut.GetLogsAsync(dateTo: to)).TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetLogsAsync_ShouldFilterByIpAddress()
    {
        await _sut.LogActivityAsync(_user1.Id, "Login", "From office", "10.0.0.1");
        await _sut.LogActivityAsync(_user2.Id, "Login", "From home", "192.168.1.1");

        var (items, total) = await _sut.GetLogsAsync(ipAddress: "10.0.0");
        total.Should().Be(1);
        items.Should().AllSatisfy(l => l.IpAddress.Should().Contain("10.0.0"));
    }

    [Fact]
    public async Task GetLogsAsync_ShouldPaginate()
    {
        for (var i = 0; i < 10; i++)
            await _sut.LogActivityAsync(_user1.Id, $"Action{i}", $"Log {i}", "1.1.1.1");

        var (page1, total) = await _sut.GetLogsAsync(page: 1, pageSize: 3);
        total.Should().Be(10);
        page1.Should().HaveCount(3);

        (await _sut.GetLogsAsync(page: 2, pageSize: 3)).Items.Should().HaveCount(3);
        (await _sut.GetLogsAsync(page: 4, pageSize: 3)).Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetLogsAsync_ShouldCombineFilters()
    {
        await _sut.LogActivityAsync(_user1.Id, "Login", "U1 login", "10.0.0.1");
        await _sut.LogActivityAsync(_user1.Id, "Logout", "U1 logout", "10.0.0.1");
        await _sut.LogActivityAsync(_user2.Id, "Login", "U2 login", "192.168.1.1");

        var (items, total) = await _sut.GetLogsAsync(
            userId: _user1.Id,
            action: "Login",
            ipAddress: "10.0.0");

        total.Should().Be(1);
        items.Should().ContainSingle(l => l.Action == "Login" && l.UserId == _user1.Id);
    }

    [Fact]
    public async Task GetLogsAsync_ShouldReturnEmpty_WhenNoMatch()
    {
        var (items, total) = await _sut.GetLogsAsync(userId: Guid.NewGuid());

        total.Should().Be(0);
        items.Should().BeEmpty();
    }
}
