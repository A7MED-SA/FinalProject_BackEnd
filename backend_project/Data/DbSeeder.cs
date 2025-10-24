using Microsoft.AspNetCore.Identity;
using backend_project.Models;

namespace backend_project.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

        // Seed Roles
        await SeedRolesAsync(roleManager);

        // Seed Admin User
        await SeedAdminUserAsync(userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<Role> roleManager)
    {
        string[] roleNames = { "Admin", "Teacher", "Student" };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new Role(roleName)
                {
                    Description = $"{roleName} role",
                    IsActive = true
                };
                await roleManager.CreateAsync(role);
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<User> userManager)
    {
        var adminEmail = "admin@lms.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new User
            {
                Name = "System Administrator",
                Email = adminEmail,
                UserName = adminEmail,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
