namespace Athary.Domain.Enums;

public enum ActivityLogAction
{
    Register = 1,
    Login = 2,
    LoginFailed = 3,
    Logout = 4,
    OAuthLogin = 5,
    EmailVerified = 10,
    EmailVerificationSent = 11,
    PasswordReset = 20,
    PasswordResetRequested = 21,
    PasswordChanged = 22,
    ProfileUpdated = 30,
    ProfilePictureChanged = 31,
    SessionRevoked = 40,
    AllSessionsRevoked = 41,
    AccountLocked = 50,
    AccountUnlocked = 51,
    AccountDeactivated = 52,
    AccountReactivated = 53,
    Other = 99
}
