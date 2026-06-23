using Athary.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Data.Seeders;

public static class TestimonialSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var existingUserIds = await context.Testimonials
            .Select(t => t.UserId)
            .ToListAsync();

        var users = await context.Users
            .Where(u => u.DeletedAt == null && !existingUserIds.Contains(u.Id))
            .OrderBy(u => u.Id)
            .Take(3)
            .ToListAsync();

        if (users.Count == 0)
            return;

        var testimonials = new List<Testimonial>
        {
            new Testimonial
            {
                Content = "منصة رائعة، تعلمت الكثير عن التاريخ الإسلامي بطريقة ممتعة وتفاعلية. المحتوى عالي الجودة والمدربون متميزون.",
                Rating = 5,
                UserId = users[0].Id,
                IsApproved = true,
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow
            }
        };

        if (users.Count > 1)
        {
            testimonials.Add(new Testimonial
            {
                Content = "المدربون محترفون والمحتوى عالي الجودة. أنصح بها بشدة لمن يريد تعلم الإسلام بشكل صحيح.",
                Rating = 5,
                UserId = users[1].Id,
                IsApproved = true,
                DisplayOrder = 2,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (users.Count > 2)
        {
            testimonials.Add(new Testimonial
            {
                Content = "تجربة تعليمية فريدة من نوعها. المنصة سهلة الاستخدام والمحتوى ثري ومتنوع. شكراً لكم.",
                Rating = 4,
                UserId = users[2].Id,
                IsApproved = true,
                DisplayOrder = 3,
                CreatedAt = DateTime.UtcNow
            });
        }

        context.Testimonials.AddRange(testimonials);
        await context.SaveChangesAsync();
    }
}
