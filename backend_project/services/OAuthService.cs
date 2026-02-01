using backend_project.DTOs.Auth;
using backend_project.Models;
using backend_project.Services.Interfaces;
using Google.Apis.Auth;
using backend_project.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace backend_project.Services;

public class OAuthService : IOAuthService
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

    public async Task<AuthResponseDto> AuthenticateGoogleAsync(string idToken, string ipAddress, string userAgent)
    {
        try
        {
            // Validate Google ID token
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _configuration["Authentication:Google:ClientId"]! }
            });

            if (payload == null || string.IsNullOrEmpty(payload.Email))
                throw new UnauthorizedAccessException("Invalid Google token");

            // Find or create user
            var user = await FindOrCreateOAuthUserAsync(
                payload.Email,
                payload.GivenName ?? "User",
                payload.FamilyName ?? "",
                "Google",
                payload.Subject
            );

            // Generate tokens and create session
            return await GenerateAuthResponseAsync(user, ipAddress, userAgent, "Google OAuth");
        }
        catch (Exception ex)
        {
            throw new UnauthorizedAccessException($"Google authentication failed: {ex.Message}");
        }
    }

    public async Task<AuthResponseDto> AuthenticateMicrosoftAsync(string idToken, string ipAddress, string userAgent)
    {
        try
        {
            // Validate Microsoft ID token by calling Microsoft Graph API
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);
            var response = await _httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");

            if (!response.IsSuccessStatusCode)
                throw new UnauthorizedAccessException("Invalid Microsoft token");

            var content = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<MicrosoftUserInfo>(content);

            if (userInfo == null || string.IsNullOrEmpty(userInfo.mail ?? userInfo.userPrincipalName))
                throw new UnauthorizedAccessException("Could not retrieve user email from Microsoft");

            var email = userInfo.mail ?? userInfo.userPrincipalName!;
            var firstName = userInfo.givenName ?? "User";
            var lastName = userInfo.surname ?? "";

            // Find or create user
            var user = await FindOrCreateOAuthUserAsync(
                email,
                firstName,
                lastName,
                "Microsoft",
                userInfo.id!
            );

            // Generate tokens and create session
            return await GenerateAuthResponseAsync(user, ipAddress, userAgent, "Microsoft OAuth");
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
        string providerKey)
    {
        // Check if user exists
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            // Create new user
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = email,
                Name = firstName + " " + lastName,
                EmailConfirmed = true, // OAuth users are pre-verified
                IsActive = true, // Activate immediately for OAuth users
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                throw new Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            // Assign default role (Student)
            await _userManager.AddToRoleAsync(user, "Student");
        }

        // Check if external login already exists
        var existingLogin = await _userManager.FindByLoginAsync(provider, providerKey);
        
        if (existingLogin == null)
        {
            // Add external login
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
    string loginMethod)
{
    // 1️⃣ Generate refresh token
    var refreshToken = _tokenService.GenerateRefreshToken();
    var refreshTokenHash = _tokenService.HashToken(refreshToken);

    // 2️⃣ Create session FIRST
    var session = await _sessionService.CreateSessionAsync(
        user,
        refreshTokenHash,
        ipAddress,
        userAgent
    );

    // 3️⃣ Generate access token using REAL session.Id
    var accessToken = await _tokenService.GenerateAccessTokenAsync(
        user,
        session.Id
    );

    // 4️⃣ Log activity
    await _activityLogService.LogActivityAsync(
        user.Id,
        "Login",
        $"User logged in via {loginMethod}",
        ipAddress,
        userAgent
    );

    // 5️⃣ Get roles
    var roles = await _userManager.GetRolesAsync(user);

    // 6️⃣ Return response
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
            Name = user.Name,
            ProfilePictureUrl = null, // File-based: generate URL from ProfileImageFile if needed
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            Roles = roles.ToList()
        }
    };
}


    // Helper class for Microsoft Graph API response
    private class MicrosoftUserInfo
    {
        public string? id { get; set; }
        public string? mail { get; set; }
        public string? userPrincipalName { get; set; }
        public string? givenName { get; set; }
        public string? surname { get; set; }
    }
}
