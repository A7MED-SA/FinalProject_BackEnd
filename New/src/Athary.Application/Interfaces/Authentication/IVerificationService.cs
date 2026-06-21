using Athary.Domain.Entities;
using Athary.Domain.Enums;

namespace Athary.Application.Interfaces.Authentication;

public interface IVerificationService
{
    Task<string> GenerateOtpAsync(User user, VerificationTokenType tokenType, CancellationToken cancellationToken = default);
    Task<bool> VerifyOtpAsync(string email, string otp, VerificationTokenType tokenType, CancellationToken cancellationToken = default);
    Task InvalidatePreviousTokensAsync(Guid userId, VerificationTokenType tokenType, CancellationToken cancellationToken = default);
    Task MarkTokenAsUsedAsync(Guid userId, VerificationTokenType tokenType, CancellationToken cancellationToken = default);
}
