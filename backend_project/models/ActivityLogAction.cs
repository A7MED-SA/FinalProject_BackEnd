namespace backend_project.Models;

public enum ActivityLogAction
{
    // Authentication Actions
    Register = 1,
    Login = 2,
    LoginFailed = 3,
    Logout = 4,
    OAuthLogin = 5,
    
    // Verification Actions
    EmailVerified = 10,
    EmailVerificationSent = 11,
    
    // Password Actions
    PasswordReset = 20,
    PasswordResetRequested = 21,
    PasswordChanged = 22,
    
    // Profile Actions
    ProfileUpdated = 30,
    ProfilePictureChanged = 31,
    
    // Session Actions
    SessionRevoked = 40,
    AllSessionsRevoked = 41,
    
    // Security Actions
    AccountLocked = 50,
    AccountUnlocked = 51,
    AccountDeactivated = 52,
    AccountReactivated = 53,
    
    // Other
    Other = 99
}
