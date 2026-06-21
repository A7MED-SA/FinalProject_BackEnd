using System.Security.Cryptography;
using System.Text;
using Athary.Application.Interfaces.Authentication;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Athary.Infrastructure.Services.Authentication;

public class VerificationService : IVerificationService
{
    private readonly IRepository<VerificationToken> _verificationTokenRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly int _otpExpirationMinutes = 15;
    private readonly int _otpLength = 6;

    public VerificationService(
        IRepository<VerificationToken> verificationTokenRepo,
        IUnitOfWork unitOfWork,
        UserManager<User> userManager,
        IEmailService emailService)
    {
        _verificationTokenRepo = verificationTokenRepo;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<string> GenerateOtpAsync(User user, VerificationTokenType tokenType, CancellationToken cancellationToken = default)
    {
        await InvalidatePreviousTokensAsync(user.Id, tokenType, cancellationToken);

        var otp = GenerateRandomOtp();
        var otpHash = HashOtp(otp);

        var verificationToken = new VerificationToken
        {
            UserId = user.Id,
            TokenHash = otpHash,
            TokenType = tokenType,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_otpExpirationMinutes),
            CreatedAt = DateTime.UtcNow
        };

        await _verificationTokenRepo.AddAsync(verificationToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await SendOtpEmailAsync(user, otp, tokenType);

        return otp;
    }

    public async Task<bool> VerifyOtpAsync(string email, string otp, VerificationTokenType tokenType, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return false;

        var otpHash = HashOtp(otp);

        var token = await _verificationTokenRepo.FirstOrDefaultAsync(
            vt =>
                vt.UserId == user.Id &&
                vt.TokenType == tokenType &&
                vt.TokenHash == otpHash &&
                vt.UsedAt == null &&
                vt.ExpiresAt > DateTime.UtcNow,
            cancellationToken: cancellationToken);

        return token != null;
    }

    public async Task InvalidatePreviousTokensAsync(Guid userId, VerificationTokenType tokenType, CancellationToken cancellationToken = default)
    {
        var previousTokens = await _verificationTokenRepo.FindAsync(
            vt => vt.UserId == userId && vt.TokenType == tokenType && vt.UsedAt == null,
            cancellationToken);

        foreach (var token in previousTokens)
        {
            token.UsedAt = DateTime.UtcNow;
            await _verificationTokenRepo.UpdateAsync(token, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkTokenAsUsedAsync(Guid userId, VerificationTokenType tokenType, CancellationToken cancellationToken = default)
    {
        var token = await _verificationTokenRepo.FirstOrDefaultAsync(
            vt => vt.UserId == userId && vt.TokenType == tokenType && vt.UsedAt == null,
            cancellationToken: cancellationToken);

        if (token != null)
        {
            token.UsedAt = DateTime.UtcNow;
            await _verificationTokenRepo.UpdateAsync(token, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private string GenerateRandomOtp()
    {
        var number = RandomNumberGenerator.GetInt32(1_000_000);
        return number.ToString($"D{_otpLength}");
    }

    private string HashOtp(string otp)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(otp));
        return Convert.ToBase64String(hashBytes);
    }

    private async Task SendOtpEmailAsync(User user, string otp, VerificationTokenType tokenType)
    {
        if (tokenType == VerificationTokenType.EmailVerification)
        {
            await _emailService.SendEmailVerificationAsync(user.Email!, user.FullName, otp);
        }
        else if (tokenType == VerificationTokenType.PasswordReset)
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email!, user.FullName, otp);
        }
    }
}
