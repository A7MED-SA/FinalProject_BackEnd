using backend_project.Data;
using backend_project.Models;
using backend_project.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace backend_project.Services;

public class VerificationService : IVerificationService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly int _otpExpirationMinutes = 15;
    private readonly int _otpLength = 6;

    public VerificationService(
        ApplicationDbContext context,
        UserManager<User> userManager,
        IEmailService emailService)
    {
        _context = context;
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<string> GenerateOtpAsync(User user, VerificationTokenType tokenType)
    {
        // Invalidate previous tokens
        await InvalidatePreviousTokensAsync(user.Id, tokenType);

        // Generate 6-digit OTP
        var otp = GenerateRandomOtp();

        // Hash OTP
        var otpHash = HashOtp(otp);

        // Create verification token
        var verificationToken = new VerificationToken
        {
            UserId = user.Id,
            TokenHash = otpHash,
            TokenType = tokenType,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_otpExpirationMinutes),
            CreatedAt = DateTime.UtcNow
        };

        _context.VerificationTokens.Add(verificationToken);
        await _context.SaveChangesAsync();

        // Send email
        await SendOtpEmailAsync(user, otp, tokenType);

        return otp; // Return unhashed OTP (only for testing, in production you might not return this)
    }

    public async Task<bool> VerifyOtpAsync(string email, string otp, VerificationTokenType tokenType)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return false;

        var otpHash = HashOtp(otp);

        var token = await _context.VerificationTokens
        .Where(vt =>
            vt.UserId == user.Id &&
            vt.TokenType == tokenType &&
            vt.TokenHash == otpHash &&
            vt.UsedAt == null &&
            vt.ExpiresAt > DateTime.UtcNow)
        .OrderByDescending(vt => vt.CreatedAt)
        .FirstOrDefaultAsync();


        return token != null;
    }

    public async Task InvalidatePreviousTokensAsync(Guid userId, VerificationTokenType tokenType)
    {
        var previousTokens = await _context.VerificationTokens
            .Where(vt => vt.UserId == userId && vt.TokenType == tokenType && vt.UsedAt == null)
            .ToListAsync();

        foreach (var token in previousTokens)
        {
            token.UsedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task MarkTokenAsUsedAsync(Guid userId, VerificationTokenType tokenType)
    {
        var token = await _context.VerificationTokens
            .Where(vt => vt.UserId == userId && vt.TokenType == tokenType && vt.UsedAt == null)
            .OrderByDescending(vt => vt.CreatedAt)
            .FirstOrDefaultAsync();

        if (token != null)
        {
            token.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private string GenerateRandomOtp()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var number = BitConverter.ToUInt32(bytes, 0);
        var otp = (number % 1000000).ToString($"D{_otpLength}");
        return otp;
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
