using backend_project.DTOs.Auth;
using backend_project.DTOs;
using backend_project.Models;
using backend_project.Data;
using backend_project.Configuration;
using Microsoft.Extensions.Options;
using backend_project.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace backend_project.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ISessionService _sessionService;
    private readonly JwtSettings _jwtSettings;
    private readonly IVerificationService _verificationService;
    private readonly IActivityLogService _activityLogService;
    private readonly IConfiguration _configuration;
    private readonly IFileService _fileService;

    public AuthenticationService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtOptions,
        ApplicationDbContext context,
        ISessionService sessionService,
        IVerificationService verificationService,
        IActivityLogService activityLogService,
        IConfiguration configuration,
        IFileService fileService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _sessionService = sessionService;
        _jwtSettings = jwtOptions.Value;
        _context = context;
        _verificationService = verificationService;
        _activityLogService = activityLogService;
        _configuration = configuration;
        _fileService = fileService;
    }

    public async Task<RegisterResponseDto> RegisterAsync(
        RegisterDto dto,
        string ipAddress,
        IFormFile? profilePicture)
    {
        // Check duplicate email
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new Exception("User with this email already exists");

        string? profilePictureUrl = null;

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Upload profile picture
            if (profilePicture != null)
            {
                profilePictureUrl =
                    await _fileService.UploadFileAsync(profilePicture, "profiles");
            }

            // Create User (NO Id assignment)
            var user = new User
            {
                Email = dto.Email,
                UserName = dto.Email,
                Name = dto.Name,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                ProfilePictureUrl = profilePictureUrl,
                EmailConfirmed = false,
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ",
                    result.Errors.Select(e => e.Description)));

            // Assign default role
            await _userManager.AddToRoleAsync(user, "Student");

            // Add phone
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                user.UserPhones.Add(new UserPhone
                {
                    UserId = user.Id,
                    PhoneNumber = dto.PhoneNumber,
                    Type = PhoneType.Primary,
                    IsDefault = true,
                    IsVerified = false
                });
            }

            // Add address
            if (!string.IsNullOrWhiteSpace(dto.Country) &&
                !string.IsNullOrWhiteSpace(dto.City) &&
                !string.IsNullOrWhiteSpace(dto.PostalCode))
            {
                user.Addresses.Add(new Address
                {
                    UserId = user.Id,
                    Country = dto.Country,
                    City = dto.City,
                    StreetLine1 = dto.StreetLine1 ?? "",
                    PostalCode = dto.PostalCode,
                    Type = AddressType.Home,
                    IsDefault = true
                });
            }

            await _context.SaveChangesAsync();

            // Generate & send email OTP
            await _verificationService.GenerateOtpAsync(
                user,
                VerificationTokenType.EmailVerification
            );

            // Log activity
            await _activityLogService.LogActivityAsync(
                user.Id,
                "Register",
                "User registered successfully",
                ipAddress
            );

            await transaction.CommitAsync();

            return new RegisterResponseDto
            {
                UserId = user.Id,
                Email = user.Email!,
                Message = "User registered successfully. Please verify your email."
            };
        }
        catch
        {
            await transaction.RollbackAsync();

            if (!string.IsNullOrEmpty(profilePictureUrl))
                await _fileService.DeleteFileAsync(profilePictureUrl);

            throw;
        }
    }

    public async Task<AuthResponseDto> LoginAsync(
    LoginDto dto,
    string ipAddress,
    string userAgent)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password");

        if (!user.IsActive || !user.EmailConfirmed)
            throw new UnauthorizedAccessException("Account is not active. Please verify your email.");

        if (await _userManager.IsLockedOutAsync(user))
            throw new UnauthorizedAccessException("Account is locked. Please try again later.");

        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            dto.Password,
            lockoutOnFailure: true
        );

        if (!result.Succeeded)
        {
            await _activityLogService.LogActivityAsync(
                user.Id,
                "LoginFailed",
                "Failed login attempt",
                ipAddress,
                userAgent
            );

            if (result.IsLockedOut)
                throw new UnauthorizedAccessException("Account locked due to multiple failed attempts");

            throw new UnauthorizedAccessException("Invalid email or password");
        }

        user.LastLogin = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

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

        // 3️⃣ Generate access token using session.Id
        var accessToken = await _tokenService.GenerateAccessTokenAsync(
            user,
            session.Id
        );

        await _activityLogService.LogActivityAsync(
            user.Id,
            "Login",
            "User logged in successfully",
            ipAddress,
            userAgent
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
                Name = user.Name,
                ProfilePictureUrl = user.ProfilePictureUrl,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList()
            }
        };
    }


    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress, string userAgent)
    {
        var refreshTokenHash = _tokenService.HashToken(refreshToken);
        var session = await _sessionService.GetSessionByRefreshTokenHashAsync(refreshTokenHash);

        if (session == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        // Validate session
        if (!await _sessionService.ValidateSessionAsync(session.Id, ipAddress, userAgent))
            throw new UnauthorizedAccessException("Session validation failed");

        var user = session.User;

        // Check if user is still active
        if (!user.IsActive || !user.EmailConfirmed)
            throw new UnauthorizedAccessException("Account is no longer active");

        // Generate new tokens
        var newAccessToken = await _tokenService.GenerateAccessTokenAsync(user, session.Id);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Hash new tokens
        var newAccessTokenHash = _tokenService.HashToken(newAccessToken);
        var newRefreshTokenHash = _tokenService.HashToken(newRefreshToken);

        // Update session with new tokens (rotation)
        await _sessionService.UpdateSessionTokensAsync(session.Id, newAccessTokenHash, newRefreshTokenHash);

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            SessionId = session.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"]!)),
            User = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email!,
                Name = user.Name,
                ProfilePictureUrl = user.ProfilePictureUrl,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList()
            }
        };
    }

    public async Task VerifyEmailAsync(VerifyEmailDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            throw new Exception("User not found");

        // Verify OTP
        if (!await _verificationService.VerifyOtpAsync(dto.Email, dto.Token, VerificationTokenType.EmailVerification))
            throw new Exception("Invalid or expired OTP");

        // Activate account
        user.EmailConfirmed = true;
        user.IsActive = true;
        await _userManager.UpdateAsync(user);

        // Mark token as used
        await _verificationService.MarkTokenAsUsedAsync(user.Id, VerificationTokenType.EmailVerification);

        // Log activity
        await _activityLogService.LogActivityAsync(
            user.Id,
            "EmailVerified",
            "Email verified successfully",
            "0.0.0.0" // No IP available in this context
        );
    }

    public async Task ResendVerificationAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            throw new Exception("User not found");

        if (user.EmailConfirmed)
            throw new Exception("Email already verified");

        await _verificationService.GenerateOtpAsync(user, VerificationTokenType.EmailVerification);
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Don't reveal that user doesn't exist
            return;
        }

        await _verificationService.GenerateOtpAsync(user, VerificationTokenType.PasswordReset);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            throw new Exception("User not found");

        // Verify OTP
        if (!await _verificationService.VerifyOtpAsync(dto.Email, dto.Token, VerificationTokenType.PasswordReset))
            throw new Exception("Invalid or expired OTP");

        // Reset password
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

        if (!result.Succeeded)
            throw new Exception($"Failed to reset password: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        // Mark token as used
        await _verificationService.MarkTokenAsUsedAsync(user.Id, VerificationTokenType.PasswordReset);

        // Revoke all sessions (force re-login)
        await _sessionService.RevokeAllUserSessionsAsync(user.Id);

        // Log activity
        await _activityLogService.LogActivityAsync(
            user.Id,
            "PasswordReset",
            "Password reset successfully",
            "0.0.0.0"
        );
    }

    public async Task LogoutAsync(Guid sessionId)
    {
        await _sessionService.RevokeSessionAsync(sessionId);
    }

    public async Task LogoutAllSessionsAsync(Guid userId, Guid currentSessionId)
    {
        await _sessionService.RevokeAllUserSessionsAsync(userId, currentSessionId);
    }
}
