using System.Net;
using System.Net.Http.Json;
using Athary.Application.Common;
using Athary.Application.DTOs.Auth;
using FluentAssertions;
using Xunit.Abstractions;

namespace Athary.API.IntegrationTests;

public sealed class AuthFlowTests : IClassFixture<AtharyApiFactory>
{
    private readonly AtharyApiFactory _factory;

    public AuthFlowTests(AtharyApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var response = await _factory.PublicClient.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = AtharyApiFactory.AdminEmail,
            Password = AtharyApiFactory.AdminPassword
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        wrapper.Should().NotBeNull();
        wrapper!.Success.Should().BeTrue();
        wrapper.Data.Should().NotBeNull();
        wrapper.Data!.AccessToken.Should().NotBeNullOrEmpty();
        wrapper.Data.RefreshToken.Should().NotBeNullOrEmpty();
        wrapper.Data.User.Email.Should().Be(AtharyApiFactory.AdminEmail);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsForbidden()
    {
        var response = await _factory.PublicClient.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = AtharyApiFactory.AdminEmail,
            Password = "WrongPassword123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AccessProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _factory.PublicClient.GetAsync("/api/profile/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AccessProtectedEndpoint_WithValidToken_ReturnsOk()
    {
        var loginResponse = await _factory.LoginAsAdminAsync();

        using var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/profile/me");
        request.Headers.Authorization = new("Bearer", loginResponse.AccessToken);

        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RefreshToken_ReturnsNewTokens()
    {
        var loginResponse = await _factory.LoginAsAdminAsync();

        var refreshResponse = await _factory.PublicClient.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenDto
        {
            RefreshToken = loginResponse.RefreshToken
        });

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var wrapper = await refreshResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        wrapper!.Success.Should().BeTrue();
        wrapper.Data!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetPublicCourseEndpoint_WithoutAuth_ReturnsOk()
    {
        var response = await _factory.PublicClient.GetAsync("/api/public/courses");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminEndpoint_WithAdminToken_ReturnsOk()
    {
        var loginResponse = await _factory.LoginAsAdminAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/system-settings");
        request.Headers.Authorization = new("Bearer", loginResponse.AccessToken);

        var response = await _factory.PublicClient.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SecurityHeaders_ShouldBePresent()
    {
        var response = await _factory.PublicClient.GetAsync("/api/public/courses");

        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");

        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.GetValues("X-Frame-Options").Should().Contain("DENY");

        response.Headers.Should().ContainKey("Referrer-Policy");
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        var response = await _factory.PublicClient.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
