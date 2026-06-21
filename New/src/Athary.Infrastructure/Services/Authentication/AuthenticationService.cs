using Athary.Application.DTOs.Auth;
using Athary.Application.Interfaces.Authentication;
using Athary.Application.Interfaces.Media;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Domain.Interfaces;
using Athary.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Athary.Infrastructure.Services.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ISessionService _sessionService;
    private readonly JwtSettings _jwtSettings;
    private readonly IVerificationService _verificationService;
    private readonly IActivityLogService _activityLogService;
    private readonly IRepository<Session> _sessionRepo;
    private readonly IRepository<VerificationToken> _verificationTokenRepo;
    private readonly IRepository<UserPhone> _userPhoneRepo;
    private readonly IRepository<Address> _addressRepo;
    private readonly IRepository<UploadedFile> _fileRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IObjectStorage _objectStorage;

    public AuthenticationService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtOptions,
        ISessionService sessionService,
        IVerificationService verificationService,
        IActivityLogService activityLogService,
        IRepository<Session> sessionRepo,
        IRepository<VerificationToken> verificationTokenRepo,
        IRepository<UserPhone> userPhoneRepo,
        IRepository<Address> addressRepo,
        IRepository<UploadedFile> fileRepo,
        IUnitOfWork unitOfWork,
        IObjectStorage objectStorage)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _sessionService = sessionService;
        _jwtSettings = jwtOptions.Value;
        _verificationService = verificationService;
        _activityLogService = activityLogService;
        _sessionRepo = sessionRepo;
        _verificationTokenRepo = verificationTokenRepo;
        _userPhoneRepo = userPhoneRepo;
        _addressRepo = addressRepo;
        _fileRepo = fileRepo;
        _unitOfWork = unitOfWork;
        _objectStorage = objectStorage;
    }

    public async Task<RegisterResponseDto> RegisterAsync(
        RegisterDto dto,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new Exception("User with this email already exists");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var user = new User
            {
                Email = dto.Email,
                UserName = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                EmailConfirmed = false,
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ",
                    result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, "Student");

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                await _userPhoneRepo.AddAsync(new UserPhone
                {
                    UserId = user.Id,
                    PhoneNumber = dto.PhoneNumber,
                    Type = PhoneType.Primary,
                    IsDefault = true,
                    IsVerified = false
                }, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(dto.Country) &&
                !string.IsNullOrWhiteSpace(dto.City) &&
                !string.IsNullOrWhiteSpace(dto.PostalCode))
            {
                await _addressRepo.AddAsync(new Address
                {
                    UserId = user.Id,
                    Country = dto.Country,
                    City = dto.City,
                    StreetLine1 = dto.StreetLine1 ?? "",
                    PostalCode = dto.PostalCode,
                    Type = "Home",
                    IsDefault = true
                }, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _verificationService.GenerateOtpAsync(
                user,
                VerificationTokenType.EmailVerification,
                cancellationToken
            );

            await _activityLogService.LogActivityAsync(
                user.Id,
                "Register",
                "User registered successfully",
                ipAddress,
                cancellationToken: cancellationToken
            );

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new RegisterResponseDto
            {
                UserId = user.Id,
                Email = user.Email!,
                Message = "User registered successfully. Please verify your email."
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<AuthResponseDto> LoginAsync(
        LoginDto dto,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password");

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
                userAgent,
                cancellationToken
            );

            if (result.IsLockedOut)
                throw new UnauthorizedAccessException("Account locked due to multiple failed attempts");

            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!user.IsActive || !user.EmailConfirmed)
            throw new UnauthorizedAccessException("Account is not active. Please verify your email.");

        user.LastLogin = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

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
            "User logged in successfully",
            ipAddress,
            userAgent,
            cancellationToken
        );

        var roles = await _userManager.GetRolesAsync(user);

        var profilePictureUrl = await GetProfilePictureUrlAsync(user, cancellationToken);

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
                ProfilePictureUrl = profilePictureUrl,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList()
            }
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress, string userAgent, CancellationToken cancellationToken = default)
    {
        var refreshTokenHash = _tokenService.HashToken(refreshToken);
        var session = await _sessionService.GetSessionByRefreshTokenHashAsync(refreshTokenHash, cancellationToken);

        if (session == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        if (!await _sessionService.ValidateSessionAsync(session.Id, ipAddress, userAgent, cancellationToken))
            throw new UnauthorizedAccessException("Session validation failed");

        var user = session.User;

        if (!user.IsActive || !user.EmailConfirmed)
            throw new UnauthorizedAccessException("Account is no longer active");

        var newAccessToken = await _tokenService.GenerateAccessTokenAsync(user, session.Id);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        var newAccessTokenHash = _tokenService.HashToken(newAccessToken);
        var newRefreshTokenHash = _tokenService.HashToken(newRefreshToken);

        await _sessionService.UpdateSessionTokensAsync(session.Id, newRefreshTokenHash, cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);

        var profilePictureUrl = await GetProfilePictureUrlAsync(user, cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            SessionId = session.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            User = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                ProfilePictureUrl = profilePictureUrl,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList()
            }
        };
    }

    public async Task VerifyEmailAsync(VerifyEmailDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            throw new Exception("User not found");

        if (!await _verificationService.VerifyOtpAsync(dto.Email, dto.Token, VerificationTokenType.EmailVerification, cancellationToken))
            throw new Exception("Invalid or expired OTP");

        user.EmailConfirmed = true;
        user.IsActive = true;
        await _userManager.UpdateAsync(user);

        await _verificationService.MarkTokenAsUsedAsync(user.Id, VerificationTokenType.EmailVerification, cancellationToken);

        await _activityLogService.LogActivityAsync(
            user.Id,
            "EmailVerified",
            "Email verified successfully",
            "0.0.0.0",
            cancellationToken: cancellationToken
        );
    }

    public async Task ResendVerificationAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            throw new Exception("User not found");

        if (user.EmailConfirmed)
            throw new Exception("Email already verified");

        await _verificationService.GenerateOtpAsync(user, VerificationTokenType.EmailVerification, cancellationToken);
    }

    public async Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return;
        }

        await _verificationService.GenerateOtpAsync(user, VerificationTokenType.PasswordReset, cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            throw new Exception("User not found");

        if (!await _verificationService.VerifyOtpAsync(dto.Email, dto.Token, VerificationTokenType.PasswordReset, cancellationToken))
            throw new Exception("Invalid or expired OTP");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

        if (!result.Succeeded)
            throw new Exception($"Failed to reset password: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        await _verificationService.MarkTokenAsUsedAsync(user.Id, VerificationTokenType.PasswordReset, cancellationToken);

        await _sessionService.RevokeAllUserSessionsAsync(user.Id, cancellationToken: cancellationToken);

        await _activityLogService.LogActivityAsync(
            user.Id,
            "PasswordReset",
            "Password reset successfully",
            "0.0.0.0",
            cancellationToken: cancellationToken
        );
    }

    public async Task LogoutAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        await _sessionService.RevokeSessionAsync(sessionId, cancellationToken);
    }

    public async Task LogoutAllSessionsAsync(Guid userId, Guid currentSessionId, CancellationToken cancellationToken = default)
    {
        await _sessionService.RevokeAllUserSessionsAsync(userId, currentSessionId, cancellationToken);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
                   ?? throw new KeyNotFoundException("User not found");

        var result = await _userManager.ChangePasswordAsync(
            user,
            dto.CurrentPassword,
            dto.NewPassword
        );

        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join(", ", result.Errors.Select(e => e.Description))
            );
    }

    private async Task<string?> GetProfilePictureUrlAsync(User user, CancellationToken cancellationToken)
    {
        if (user.ProfileImageFileId is null)
            return null;

        var file = await _fileRepo.GetByIdAsync(user.ProfileImageFileId.Value, cancellationToken);
        if (file is null)
            return null;

        return _objectStorage.GetPublicUrl(file.Bucket, file.FilePath);
    }
}
