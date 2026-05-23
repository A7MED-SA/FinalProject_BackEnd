using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using backend_project.Data;
using backend_project.Models;

namespace CommerceTests.Helpers;

public static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"CommerceTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    public static User CreateUser(Guid? id = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            UserName = "testuser"
        };
    }

    public static Course CreateCourse(Guid? id = null, decimal price = 100, Guid? creatorId = null)
    {
        return new Course
        {
            Id = id ?? Guid.NewGuid(),
            Title = "Test Course",
            Slug = "test-course",
            Description = "Test Description",
            Price = price,
            Status = CourseStatus.Published,
            CreatedBy = creatorId ?? Guid.NewGuid()
        };
    }

    public static Cart CreateCart(Guid userId)
    {
        return new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId
        };
    }

    public static CartItem CreateCartItem(Guid cartId, Guid courseId, decimal priceSnapshot)
    {
        return new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cartId,
            CourseId = courseId,
            PriceSnapshot = priceSnapshot,
            AddedAt = DateTime.UtcNow
        };
    }

    public static PaymentMethod CreatePaymentMethod(Guid? id = null, bool isActive = true)
    {
        return new PaymentMethod
        {
            Id = id ?? Guid.NewGuid(),
            Name = "Test Wallet",
            Provider = "Mock",
            Type = backend_project.Models.PaymentMethodType.DigitalWallet,
            IsActive = isActive
        };
    }
}
