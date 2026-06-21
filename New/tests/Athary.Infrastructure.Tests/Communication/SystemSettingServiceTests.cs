using Athary.Application.DTOs.Communication;
using Athary.Application.Interfaces.Authentication;
using Athary.Domain.Entities;
using Athary.Infrastructure.Repositories;
using Athary.Infrastructure.Services.Communication;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace Athary.Infrastructure.Tests.Communication;

public sealed class SystemSettingServiceTests : SqliteTestBase
{
    private readonly SystemSettingService _sut;
    private readonly Mock<IActivityLogService> _activityLogMock;
    private readonly IMemoryCache _cache;
    private readonly User _admin;
    private readonly Guid _adminId;

    public SystemSettingServiceTests()
    {
        _admin = new User { FirstName = "Admin", LastName = "A", Email = "a@a.com", UserName = "aa" };
        Context.Users.Add(_admin);
        Context.SaveChanges();
        _adminId = _admin.Id;

        var settingRepo = new GenericRepository<SystemSetting>(Context);
        var uow = new UnitOfWork(Context);
        _cache = new MemoryCache(new MemoryCacheOptions());
        _activityLogMock = new Mock<IActivityLogService>();

        _sut = new SystemSettingService(settingRepo, uow, _cache, _activityLogMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateSetting()
    {
        var result = await _sut.CreateAsync(new CreateSettingRequest
        {
            Key = "SiteName",
            Value = "Athary",
            Group = "General",
            DataType = "String"
        }, _adminId);

        result.Key.Should().Be("SiteName");
        result.Value.Should().Be("Athary");
        result.Group.Should().Be("General");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDuplicateKey()
    {
        await _sut.CreateAsync(new CreateSettingRequest
        {
            Key = "SiteName", Value = "Athary", DataType = "String"
        }, _adminId);

        await FluentActions.Invoking(() => _sut.CreateAsync(new CreateSettingRequest
        {
            Key = "SiteName", Value = "New", DataType = "String"
        }, _adminId)).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenInvalidDataType()
    {
        await FluentActions.Invoking(() => _sut.CreateAsync(new CreateSettingRequest
        {
            Key = "BadType", Value = "test", DataType = "InvalidType"
        }, _adminId)).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldValidateIntegerValue()
    {
        await FluentActions.Invoking(() => _sut.CreateAsync(new CreateSettingRequest
        {
            Key = "MaxUsers", Value = "not-a-number", DataType = "Integer"
        }, _adminId)).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldAcceptValidIntegerValue()
    {
        var result = await _sut.CreateAsync(new CreateSettingRequest
        {
            Key = "MaxUsers", Value = "100", DataType = "Integer"
        }, _adminId);

        result.Value.Should().Be("100");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllSettings()
    {
        await _sut.CreateAsync(new CreateSettingRequest { Key = "K1", Value = "V1", DataType = "String" }, _adminId);
        await _sut.CreateAsync(new CreateSettingRequest { Key = "K2", Value = "V2", DataType = "String" }, _adminId);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByGroup()
    {
        await _sut.CreateAsync(new CreateSettingRequest { Key = "K1", Value = "V1", Group = "A", DataType = "String" }, _adminId);
        await _sut.CreateAsync(new CreateSettingRequest { Key = "K2", Value = "V2", Group = "B", DataType = "String" }, _adminId);

        var result = await _sut.GetAllAsync("A");

        result.Should().HaveCount(1);
        result[0].Key.Should().Be("K1");
    }

    [Fact]
    public async Task GetByKeyAsync_ShouldReturnSetting()
    {
        await _sut.CreateAsync(new CreateSettingRequest { Key = "SiteName", Value = "Athary", DataType = "String" }, _adminId);

        var result = await _sut.GetByKeyAsync("SiteName");

        result.Value.Should().Be("Athary");
    }

    [Fact]
    public async Task GetByKeyAsync_ShouldThrow_WhenNotFound()
    {
        await FluentActions.Invoking(() => _sut.GetByKeyAsync("NonExistent"))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateValue()
    {
        await _sut.CreateAsync(new CreateSettingRequest { Key = "SiteName", Value = "Old", DataType = "String" }, _adminId);

        var result = await _sut.UpdateAsync("SiteName", new UpdateSettingRequest { Value = "New" }, _adminId);

        result.Value.Should().Be("New");
        _activityLogMock.Verify(a => a.LogActivityAsync(
            _adminId, "SettingUpdated", It.IsAny<string>(), It.IsAny<string>(),
            null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        await FluentActions.Invoking(() => _sut.UpdateAsync("NonExistent", new UpdateSettingRequest { Value = "X" }, _adminId))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldValidateDataType()
    {
        await _sut.CreateAsync(new CreateSettingRequest { Key = "Count", Value = "42", DataType = "Integer" }, _adminId);

        await FluentActions.Invoking(() => _sut.UpdateAsync("Count", new UpdateSettingRequest { Value = "not-int" }, _adminId))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveSetting()
    {
        await _sut.CreateAsync(new CreateSettingRequest { Key = "Temp", Value = "X", DataType = "String" }, _adminId);

        await _sut.DeleteAsync("Temp");

        var settings = await _sut.GetAllAsync();
        settings.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        await FluentActions.Invoking(() => _sut.DeleteAsync("NonExistent"))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Settings_ShouldBeCached()
    {
        await _sut.CreateAsync(new CreateSettingRequest { Key = "Cached", Value = "Yes", DataType = "String" }, _adminId);

        await _sut.GetByKeyAsync("Cached");

        var setting = await Context.SystemSettings.FirstAsync();
        setting.Value = "No";
        await Context.SaveChangesAsync();

        var second = await _sut.GetByKeyAsync("Cached");
        second.Value.Should().Be("Yes");
    }

    [Fact]
    public async Task Create_ShouldInvalidateCache()
    {
        await _sut.CreateAsync(new CreateSettingRequest { Key = "New", Value = "V", DataType = "String" }, _adminId);

        var fresh = await _sut.GetByKeyAsync("New");
        fresh.Value.Should().Be("V");
    }

    [Fact]
    public async Task Update_ShouldInvalidateCache()
    {
        await _sut.CreateAsync(new CreateSettingRequest { Key = "Updatable", Value = "Old", DataType = "String" }, _adminId);

        var result = await _sut.UpdateAsync("Updatable", new UpdateSettingRequest { Value = "New" }, _adminId);
        result.Value.Should().Be("New");

        var fresh = await _sut.GetByKeyAsync("Updatable");
        fresh.Value.Should().Be("New");
    }

    [Fact]
    public async Task Delete_ShouldInvalidateCache()
    {
        await _sut.CreateAsync(new CreateSettingRequest { Key = "DeleteMe", Value = "X", DataType = "String" }, _adminId);

        await _sut.DeleteAsync("DeleteMe");

        var settings = await _sut.GetAllAsync();
        settings.Should().BeEmpty();
    }
}
