using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.Models;
using backend_project.Services.Implementations;
using CommerceTests.Helpers;

namespace CommerceTests;

public class CartServiceTests
{
    private static async Task<(ApplicationDbContext ctx, Guid userId)> SetupCartTest(int courseCount = 1)
    {
        var context = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var user = TestDbContextFactory.CreateUser(userId);
        context.Users.Add(user);

        for (int i = 0; i < courseCount; i++)
        {
            var creator = TestDbContextFactory.CreateUser();
            var course = TestDbContextFactory.CreateCourse(creatorId: creator.Id);
            context.Users.Add(creator);
            context.Courses.Add(course);
        }

        await context.SaveChangesAsync();
        return (context, userId);
    }

    [Fact]
    public async Task AddItem_ValidCourse_AddsToCart()
    {
        var (context, userId) = await SetupCartTest();
        var course = await context.Courses.FirstAsync();

        var service = new CartService(context, null!);
        var result = await service.AddItemAsync(userId, course.Id);

        result.Should().NotBeNull();
        result.CourseId.Should().Be(course.Id);

        var cart = await context.Carts.Include(c => c.CartItems).FirstAsync(c => c.UserId == userId);
        cart.CartItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddItem_DuplicateCourse_ThrowsException()
    {
        var (context, userId) = await SetupCartTest();
        var course = await context.Courses.FirstAsync();

        var service = new CartService(context, null!);
        await service.AddItemAsync(userId, course.Id);
        await FluentActions.Awaiting(() => service.AddItemAsync(userId, course.Id))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Course is already in your cart.");
    }

    [Fact]
    public async Task AddItem_AlreadyEnrolled_ThrowsException()
    {
        var (context, userId) = await SetupCartTest();
        var course = await context.Courses.FirstAsync();

        context.Enrollments.Add(new Enrollment
        {
            UserId = userId,
            CourseId = course.Id,
            Status = EnrollmentStatus.InProgress,
            Source = EnrollmentSource.Purchase
        });
        await context.SaveChangesAsync();

        var service = new CartService(context, null!);
        await FluentActions.Awaiting(() => service.AddItemAsync(userId, course.Id))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("You are already enrolled in this course.");
    }

    [Fact]
    public async Task RemoveItem_ExistingItem_RemovesFromCart()
    {
        var (context, userId) = await SetupCartTest();
        var course = await context.Courses.FirstAsync();
        var cart = TestDbContextFactory.CreateCart(userId);
        context.Carts.Add(cart);
        var cartItem = TestDbContextFactory.CreateCartItem(cart.Id, course.Id, 100);
        context.CartItems.Add(cartItem);
        await context.SaveChangesAsync();

        var service = new CartService(context, null!);
        await service.RemoveItemAsync(userId, cartItem.Id);

        var items = await context.CartItems.Where(ci => ci.Cart.UserId == userId).ToListAsync();
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCart_WithItems_ReturnsCorrectTotal()
    {
        var (context, userId) = await SetupCartTest(2);
        var courses = await context.Courses.Take(2).ToListAsync();
        var cart = TestDbContextFactory.CreateCart(userId);
        context.Carts.Add(cart);
        context.CartItems.Add(TestDbContextFactory.CreateCartItem(cart.Id, courses[0].Id, 100));
        context.CartItems.Add(TestDbContextFactory.CreateCartItem(cart.Id, courses[1].Id, 200));
        await context.SaveChangesAsync();

        var service = new CartService(context, null!);
        var result = await service.GetCartAsync(userId);

        result.Items.Should().HaveCount(2);
        result.Subtotal.Should().Be(300);
    }
}
