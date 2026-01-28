namespace FamilyHub.Modules.Auth.Domain.ValueObjects;

/// <summary>
/// Types of authentication audit events for comprehensive logging.
/// </summary>
public enum AuthAuditEventType
{
    /// <summary>
    /// Successful login attempt.
    /// </summary>
    LOGIN = 1,

    /// <summary>
    /// User logged out.
    /// </summary>
    LOGOUT = 2,

    /// <summary>
    /// Failed login attempt (wrong password, unknown email, etc.).
    /// </summary>
    FAILED_LOGIN = 3,

    /// <summary>
    /// User changed their password.
    /// </summary>
    PASSWORD_CHANGE = 4,

    /// <summary>
    /// Password was reset via email link or code.
    /// </summary>
    PASSWORD_RESET = 5,

    /// <summary>
    /// Access token was refreshed using refresh token.
    /// </summary>
    TOKEN_REFRESH = 6,

    /// <summary>
    /// Email was verified via verification link.
    /// </summary>
    EMAIL_VERIFICATION = 7,

    /// <summary>
    /// Account was locked due to too many failed attempts.
    /// </summary>
    ACCOUNT_LOCKOUT = 8,

    /// <summary>
    /// Account lockout period expired.
    /// </summary>
    ACCOUNT_UNLOCK = 9,

    /// <summary>
    /// New user registered.
    /// </summary>
    REGISTRATION = 10,

    /// <summary>
    /// User registration failed (e.g., email already exists).
    /// </summary>
    REGISTRATION_FAILED = 14,

    /// <summary>
    /// Password reset was requested.
    /// </summary>
    PASSWORD_RESET_REQUESTED = 11,

    /// <summary>
    /// Refresh token was revoked.
    /// </summary>
    TOKEN_REVOKED = 12,

    /// <summary>
    /// All sessions were logged out (logout all devices).
    /// </summary>
    LOGOUT_ALL_DEVICES = 13
}
