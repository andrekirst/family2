namespace FamilyHub.Modules.Auth.Domain.ValueObjects;

/// <summary>
/// Types of authentication audit events for comprehensive logging.
/// </summary>
public enum AuthAuditEventType
{
    /// <summary>
    /// Successful login attempt.
    /// </summary>
    Login = 1,

    /// <summary>
    /// User logged out.
    /// </summary>
    Logout = 2,

    /// <summary>
    /// Failed login attempt (wrong password, unknown email, etc.).
    /// </summary>
    FailedLogin = 3,

    /// <summary>
    /// User changed their password.
    /// </summary>
    PasswordChange = 4,

    /// <summary>
    /// Password was reset via email link or code.
    /// </summary>
    PasswordReset = 5,

    /// <summary>
    /// Access token was refreshed using refresh token.
    /// </summary>
    TokenRefresh = 6,

    /// <summary>
    /// Email was verified via verification link.
    /// </summary>
    EmailVerification = 7,

    /// <summary>
    /// Account was locked due to too many failed attempts.
    /// </summary>
    AccountLockout = 8,

    /// <summary>
    /// Account lockout period expired.
    /// </summary>
    AccountUnlock = 9,

    /// <summary>
    /// New user registered.
    /// </summary>
    Registration = 10,

    /// <summary>
    /// User registration failed (e.g., email already exists).
    /// </summary>
    RegistrationFailed = 14,

    /// <summary>
    /// Password reset was requested.
    /// </summary>
    PasswordResetRequested = 11,

    /// <summary>
    /// Refresh token was revoked.
    /// </summary>
    TokenRevoked = 12,

    /// <summary>
    /// All sessions were logged out (logout all devices).
    /// </summary>
    LogoutAllDevices = 13
}
