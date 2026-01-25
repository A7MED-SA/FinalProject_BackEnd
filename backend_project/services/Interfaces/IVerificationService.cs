using backend_project.Models;

namespace backend_project.Services.Interfaces;

public interface IVerificationService
{
    /// <summary>
    /// Generates a 6-digit OTP, hashes it, and stores it for the user
    /// </summary>
    Task<string> GenerateOtpAsync(User user, VerificationTokenType tokenType);

    /// <summary>
    /// Verifies OTP for a user and token type
    /// </summary>
    Task<bool> VerifyOtpAsync(string email, string otp, VerificationTokenType tokenType);

    /// <summary>
    /// Invalidates all previous tokens of a specific type for a user
    /// </summary>
    Task InvalidatePreviousTokensAsync(Guid userId, VerificationTokenType tokenType);

    /// <summary>
    /// Marks a verification token as used
    /// </summary>
    Task MarkTokenAsUsedAsync(Guid userId, VerificationTokenType tokenType);
}
