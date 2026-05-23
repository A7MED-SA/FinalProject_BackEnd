using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using backend_project.Models;
using backend_project.Services.Implementations;
using backend_project.Data;
using CommerceTests.Helpers;

namespace CommerceTests;

public class WishlistServiceTests
{
    private static async Task<(ApplicationDbContext ctx, Guid userId, Guid courseId)> Setup()
    {
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var creator = TestDbContextFactory.CreateUser();
        var course = TestDbContextFactory.CreateCourse(creatorId: creator.Id);
        context.Users.Add(creator);
        context.Users.Add(TestDbContextFactory.CreateUser(userId));
        context.Courses.Add(course);
        await context.SaveChangesAsync();
        return (context, userId, course.Id);
    }

    [Fact]
    public async Task AddItem_ValidCourse_AddsToWishlist()
    {
        var (context, userId, courseId) = await Setup();
        var service = new WishlistService(context);
        var result = await service.AddItemAsync(userId, courseId);

        result.CourseId.Should().Be(courseId);
        var items = await context.Wishlists.Where(w => w.UserId == userId).ToListAsync();
        items.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddItem_DuplicateCourse_ThrowsException()
    {
        var (context, userId, courseId) = await Setup();
        context.Wishlists.Add(new Wishlist { UserId = userId, CourseId = courseId });
        await context.SaveChangesAsync();

        var service = new WishlistService(context);
        await FluentActions.Awaiting(() => service.AddItemAsync(userId, courseId))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Course is already in your wishlist.");
    }

    [Fact]
    public async Task RemoveItem_ExistingItem_RemovesFromWishlist()
    {
        var (context, userId, courseId) = await Setup();
        context.Wishlists.Add(new Wishlist { UserId = userId, CourseId = courseId });
        await context.SaveChangesAsync();

        var service = new WishlistService(context);
        await service.RemoveItemAsync(userId, courseId);

        var items = await context.Wishlists.Where(w => w.UserId == userId).ToListAsync();
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWishlist_ReturnsAllItems()
    {
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var creator1 = TestDbContextFactory.CreateUser();
        var creator2 = TestDbContextFactory.CreateUser();
        var course1 = TestDbContextFactory.CreateCourse(creatorId: creator1.Id);
        var course2 = TestDbContextFactory.CreateCourse(creatorId: creator2.Id);
        context.Users.AddRange(creator1, creator2, TestDbContextFactory.CreateUser(userId));
        context.Courses.AddRange(course1, course2);
        context.Wishlists.Add(new Wishlist { UserId = userId, CourseId = course1.Id });
        context.Wishlists.Add(new Wishlist { UserId = userId, CourseId = course2.Id });
        await context.SaveChangesAsync();

        var service = new WishlistService(context);
        var result = await service.GetWishlistAsync(userId);

        result.Count.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }
}
