using System.Net.Http.Headers;
using System.Text.Json;
using Athary.Application.DTOs.Auth;
using Athary.Application.Interfaces.Authentication;
using Athary.Domain.Entities;
using Athary.Infrastructure.Settings;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Athary.Infrastructure.Services.Authentication;

public sealed class OAuthService : IOAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;
    private readonly ISessionService _sessionService;
    private readonly IActivityLogService _activityLogService;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public OAuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtOptions,
        ISessionService sessionService,
        IActivityLogService activityLogService,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _jwtSettings = jwtOptions.Value;
        _sessionService = sessionService;
        _activityLogService = activityLogService;
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<AuthResponseDto> AuthenticateGoogleAsync(string idToken, string ipAddress, string userAgent, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _configuration["Authentication:Google:ClientId"]! }
            });

            if (payload == null || string.IsNullOrEmpty(payload.Email))
                throw new UnauthorizedAccessException("Invalid Google token");

            var user = await FindOrCreateOAuthUserAsync(
                payload.Email,
                payload.GivenName ?? "User",
                payload.FamilyName ?? "",
                "Google",
                payload.Subject,
                cancellationToken
            );

            return await GenerateAuthResponseAsync(user, ipAddress, userAgent, "Google OAuth", cancellationToken);
        }
        catch (Exception ex)
        {
            throw new UnauthorizedAccessException($"Google authentication failed: {ex.Message}");
        }
    }

    public async Task<AuthResponseDto> AuthenticateMicrosoftAsync(string idToken, string ipAddress, string userAgent, CancellationToken cancellationToken = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);
            var response = await _httpClient.GetAsync("https://graph.microsoft.com/v1.0/me", cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new UnauthorizedAccessException("Invalid Microsoft token");

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var userInfo = JsonSerializer.Deserialize<MicrosoftUserInfo>(content);

            if (userInfo == null || string.IsNullOrEmpty(userInfo.mail ?? userInfo.userPrincipalName))
                throw new UnauthorizedAccessException("Could not retrieve user email from Microsoft");

            var email = userInfo.mail ?? userInfo.userPrincipalName!;
            var firstName = userInfo.givenName ?? "User";
            var lastName = userInfo.surname ?? "";

            var user = await FindOrCreateOAuthUserAsync(
                email,
                firstName,
                lastName,
                "Microsoft",
                userInfo.id!,
                cancellationToken
            );

            return await GenerateAuthResponseAsync(user, ipAddress, userAgent, "Microsoft OAuth", cancellationToken);
        }
        catch (Exception ex)
        {
            throw new UnauthorizedAccessException($"Microsoft authentication failed: {ex.Message}");
        }
    }

    private async Task<User> FindOrCreateOAuthUserAsync(
        string email,
        string firstName,
        string lastName,
        string provider,
        string providerKey,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new User
            {
                Email = email,
                UserName = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                throw new Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await _userManager.AddToRoleAsync(user, "Student");
        }

        var existingLogin = await _userManager.FindByLoginAsync(provider, providerKey);

        if (existingLogin == null)
        {
            var loginInfo = new UserLoginInfo(provider, providerKey, provider);
            var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);

            if (!addLoginResult.Succeeded)
                throw new Exception($"Failed to link external login: {string.Join(", ", addLoginResult.Errors.Select(e => e.Description))}");
        }

        return user;
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(
        User user,
        string ipAddress,
        string userAgent,
        string loginMethod,
        CancellationToken cancellationToken)
    {
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = _tokenService.HashToken(refreshToken);

        var session = await _sessionService.CreateSessionAsync(
            user,
            refreshTokenHash,
            ipAddress,
            userAgent,
            cancellationToken
        );

        var accessToken = await _tokenService.GenerateAccessTokenAsync(
            user,
            session.Id
        );

        await _activityLogService.LogActivityAsync(
            user.Id,
            "Login",
            $"User logged in via {loginMethod}",
            ipAddress,
            userAgent,
            cancellationToken
        );

        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            SessionId = session.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(
                _jwtSettings.AccessTokenExpirationMinutes
            ),
            User = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                ProfilePictureUrl = null,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList()
            }
        };
    }

    private class MicrosoftUserInfo
    {
        public string? id { get; set; }
        public string? mail { get; set; }
        public string? userPrincipalName { get; set; }
        public string? givenName { get; set; }
        public string? surname { get; set; }
    }
}
