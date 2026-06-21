using Athary.Application.Interfaces.Commerce;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Services.Commerce;
using FluentAssertions;
using Moq;

namespace Athary.Infrastructure.Tests.Commerce;

public sealed class CartServiceTests : SqliteTestBase
{
    private readonly CartService _sut;
    private readonly User _user;
    private readonly Course _course;
    private readonly Mock<ICouponService> _couponServiceMock;

    public CartServiceTests()
    {
        _user = new User { FirstName = "A", LastName = "B", Email = "a@b.com", UserName = "ab" };
        Context.Users.Add(_user);

        var cat = new Category { Name = "Cat", Slug = "cat" };
        Context.Categories.Add(cat);
        Context.SaveChanges();

        _course = new Course
        {
            Title = "Test Course",
            Slug = "test-course",
            Price = 99.99m,
            CategoryId = cat.Id,
            CreatedBy = _user.Id,
            Status = CourseStatus.Published,
            IsPublished = true
        };
        Context.Courses.Add(_course);
        Context.SaveChanges();

        _couponServiceMock = new Mock<ICouponService>();
        _sut = new CartService(Context, _couponServiceMock.Object);
    }

    [Fact]
    public async Task GetCartAsync_ShouldReturnEmptyCart_WhenNoCartExists()
    {
        var result = await _sut.GetCartAsync(_user.Id);

        result.Items.Should().BeEmpty();
        result.Subtotal.Should().Be(0);
    }

    [Fact]
    public async Task AddItemAsync_ShouldCreateCart_AndAddItem()
    {
        var result = await _sut.AddItemAsync(_user.Id, _course.Id);

        result.CourseId.Should().Be(_course.Id);
        result.CourseTitle.Should().Be(_course.Title);
        result.PriceSnapshot.Should().Be(_course.Price);

        var cart = await Context.Carts.FindAsync(Context.Carts.First().Id);
        cart.Should().NotBeNull();
        cart!.CartItems.Should().HaveCount(1);

        var item = cart.CartItems.First();
        item.CourseId.Should().Be(_course.Id);
        item.PriceSnapshot.Should().Be(_course.Price);
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrow_WhenCourseNotFound()
    {
        var badId = Guid.NewGuid();
        await FluentActions.Invoking(() => _sut.AddItemAsync(_user.Id, badId))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrow_WhenAlreadyEnrolled()
    {
        Context.Enrollments.Add(new Enrollment
        {
            UserId = _user.Id, CourseId = _course.Id, Status = EnrollmentStatus.InProgress
        });
        await Context.SaveChangesAsync();

        await FluentActions.Invoking(() => _sut.AddItemAsync(_user.Id, _course.Id))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrow_WhenDuplicateItem()
    {
        await _sut.AddItemAsync(_user.Id, _course.Id);

        await FluentActions.Invoking(() => _sut.AddItemAsync(_user.Id, _course.Id))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldRemoveItem_FromCart()
    {
        var added = await _sut.AddItemAsync(_user.Id, _course.Id);

        await _sut.RemoveItemAsync(_user.Id, added.Id);

        var cart = await _sut.GetCartAsync(_user.Id);
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldThrow_WhenItemNotInCart()
    {
        await FluentActions.Invoking(() => _sut.RemoveItemAsync(_user.Id, Guid.NewGuid()))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetCartAsync_ShouldCalculateSubtotal_WithMultipleItems()
    {
        var course2 = new Course
        {
            Title = "Course 2", Slug = "course-2", Price = 49.50m,
            CategoryId = Context.Categories.First().Id, CreatedBy = _user.Id,
            Status = CourseStatus.Published, IsPublished = true
        };
        Context.Courses.Add(course2);

        var cart = new Cart { UserId = _user.Id };
        cart.CartItems.Add(new CartItem { CourseId = _course.Id, PriceSnapshot = _course.Price });
        cart.CartItems.Add(new CartItem { CourseId = course2.Id, PriceSnapshot = course2.Price });
        Context.Carts.Add(cart);
        await Context.SaveChangesAsync();

        var result = await _sut.GetCartAsync(_user.Id);
        result.Items.Should().HaveCount(2);
        result.Subtotal.Should().Be(99.99m + 49.50m);
    }

    [Fact]
    public async Task ApplyCouponAsync_ShouldThrow_WhenCartEmpty()
    {
        await FluentActions.Invoking(() => _sut.ApplyCouponAsync(_user.Id, "SAVE10"))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ApplyCouponAsync_ShouldReturnDiscount_WhenCouponValid()
    {
        await _sut.AddItemAsync(_user.Id, _course.Id);

        _couponServiceMock
            .Setup(c => c.ValidateCouponAsync("SAVE10", 99.99m, new List<Guid> { _course.Id }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Athary.Application.DTOs.Commerce.ValidateCouponResponse
            {
                Code = "SAVE10",
                IsValid = true,
                DiscountAmount = 10.00m,
                FinalAmount = 89.99m
            });

        var result = await _sut.ApplyCouponAsync(_user.Id, "SAVE10");

        result.DiscountAmount.Should().Be(10.00m);
        result.FinalAmount.Should().Be(89.99m);
    }
}
