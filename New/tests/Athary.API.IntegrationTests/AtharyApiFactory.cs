using System.Net.Http.Json;
using Athary.Application.Common;
using Athary.Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace Athary.API.IntegrationTests;

public sealed class AtharyApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Test_12345!")
        .WithCleanUp(true)
        .Build();

    public HttpClient PublicClient { get; private set; } = null!;
    public const string AdminEmail = "admin@lms.com";
    public const string AdminPassword = "Admin@123456";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((ctx, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "TestSuperSecretKeyThatIsAtLeast32CharactersLong!",
                ["JwtSettings:Issuer"] = "AtharyTest",
                ["JwtSettings:Audience"] = "AtharyTestClient",
                ["JwtSettings:AccessTokenExpirationMinutes"] = "60",
                ["JwtSettings:RefreshTokenExpirationDays"] = "7",
                ["ConnectionStrings:Redis"] = "",
                ["EmailSettings:IsEnabled"] = "false",
                ["CorsSettings:AllowedOrigins:0"] = "http://localhost:3000"
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<Infrastructure.Data.ApplicationDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<Infrastructure.Data.ApplicationDbContext>(options =>
                options.UseSqlServer(_dbContainer.GetConnectionString()));

            // Override JwtBearer options to use the test secret key
            services.PostConfigure<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>(
                Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "AtharyTest",
                        ValidAudience = "AtharyTestClient",
                        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes("TestSuperSecretKeyThatIsAtLeast32CharactersLong!")),
                        ClockSkew = TimeSpan.Zero
                    };
                });
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Infrastructure.Data.ApplicationDbContext>();
        await db.Database.MigrateAsync();
        await Infrastructure.Data.DbSeeder.SeedAsync(scope.ServiceProvider);

        PublicClient = CreateClient();
    }

    public new async Task DisposeAsync()
    {
        PublicClient.Dispose();
        await _dbContainer.DisposeAsync();
    }

    public async Task<AuthResponseDto> LoginAsAdminAsync()
    {
        var response = await PublicClient.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = AdminEmail,
            Password = AdminPassword
        });

        response.EnsureSuccessStatusCode();

        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        return wrapper!.Data!;
    }
}
