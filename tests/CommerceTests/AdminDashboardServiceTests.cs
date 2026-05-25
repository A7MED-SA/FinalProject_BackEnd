using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using backend_project.Data;
using backend_project.Models;
using backend_project.Services.Implementations;
using CommerceTests.Helpers;

namespace CommerceTests;

public class AdminDashboardServiceTests
{
    private static AdminDashboardService CreateService(ApplicationDbContext context)
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = Mock.Of<ILogger<AdminDashboardService>>();
        return new AdminDashboardService(context, cache, logger);
    }

    [Fact]
    public async Task GetOverviewAsync_WithData_ReturnsCorrectCounts()
    {
        var context = TestDbContextFactory.Create();

        var admin = TestDbContextFactory.CreateUser();
        context.Users.Add(admin);

        // Active users
        var user1 = TestDbContextFactory.CreateUser();
        user1.IsActive = true;
        var user2 = TestDbContextFactory.CreateUser();
        user2.IsActive = true;
        context.Users.AddRange(user1, user2);

        // Courses
        var creator = TestDbContextFactory.CreateUser();
        context.Users.Add(creator);

        var course1 = TestDbContextFactory.CreateCourse(creatorId: creator.Id);
        course1.Status = CourseStatus.Published;
        course1.IsPublished = true;
        var course2 = TestDbContextFactory.CreateCourse(creatorId: creator.Id);
        course2.Status = CourseStatus.PendingReview;
        var course3 = TestDbContextFactory.CreateCourse(creatorId: Guid.NewGuid());
        course3.Status = CourseStatus.Published;
        course3.IsPublished = true;
        context.Courses.AddRange(course1, course2, course3);

        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetOverviewAsync();

        result.TotalUsers.Should().Be(2); // active users only
        result.TotalCourses.Should().Be(3);
        result.PendingCourseApprovals.Should().Be(1);
        result.ActiveInstructors.Should().Be(2); // 2 distinct creators with published courses
    }

    [Fact]
    public async Task GetOverviewAsync_EmptyDb_ReturnsZeroValues()
    {
        var context = TestDbContextFactory.Create();
        var service = CreateService(context);
        var result = await service.GetOverviewAsync();

        result.TotalUsers.Should().Be(0);
        result.TotalCourses.Should().Be(0);
        result.TotalRevenue.Should().Be(0);
        result.PendingCourseApprovals.Should().Be(0);
        result.PendingTeacherRequests.Should().Be(0);
        result.ActiveInstructors.Should().Be(0);
    }

    [Fact]
    public async Task GetMonthlyRevenueAsync_ReturnsEmptyListForNewPlatform()
    {
        var context = TestDbContextFactory.Create();
        var service = CreateService(context);
        var result = await service.GetMonthlyRevenueAsync(12);

        result.Should().HaveCount(12);
        result.Should().AllSatisfy(r =>
        {
            r.GrossAmount.Should().Be(0);
            r.NetAmount.Should().Be(0);
        });
    }

    [Fact]
    public async Task GetUserGrowthAsync_ReturnsEmptyListForNewPlatform()
    {
        var context = TestDbContextFactory.Create();
        var service = CreateService(context);
        var result = await service.GetUserGrowthAsync(6);

        result.Should().HaveCount(6);
        result.Should().AllSatisfy(r =>
        {
            r.NewUsers.Should().Be(0);
        });
    }

    [Fact]
    public async Task GetEnrollmentTrendsAsync_ReturnsEmptyListForNewPlatform()
    {
        var context = TestDbContextFactory.Create();
        var service = CreateService(context);
        var result = await service.GetEnrollmentTrendsAsync(12);

        result.Should().HaveCount(12);
        result.Should().AllSatisfy(r => r.Enrollments.Should().Be(0));
    }
}
