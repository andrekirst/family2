using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Abstractions;

/// <summary>
/// Service for authentication operations (register, login, logout, password management).
/// Orchestrates PasswordService, TokenService, and EmailService.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user with email/password authentication.
    /// Creates a personal family and sends verification email.
    /// </summary>
    /// <param name="request">Registration request details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Registration result with tokens or error.</returns>
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user with email and password.
    /// Handles lockout logic and audit logging.
    /// </summary>
    /// <param name="request">Login request details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication result with tokens or error.</returns>
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out a user by revoking the refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to revoke.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if logout was successful.</returns>
    Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out a user from all devices by revoking all refresh tokens.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of sessions logged out.</returns>
    Task<int> LogoutAllDevicesAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    /// <param name="request">Password change request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<AuthResult> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates a password reset by sending a reset link or code.
    /// </summary>
    /// <param name="request">Password reset request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result (always success to prevent email enumeration).</returns>
    Task<AuthResult> RequestPasswordResetAsync(RequestPasswordResetRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets a password using a reset token (web flow).
    /// </summary>
    /// <param name="request">Password reset details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<AuthResult> ResetPasswordWithTokenAsync(ResetPasswordWithTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets a password using a 6-digit code (mobile flow).
    /// </summary>
    /// <param name="request">Password reset details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<AuthResult> ResetPasswordWithCodeAsync(ResetPasswordWithCodeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies a user's email address.
    /// </summary>
    /// <param name="token">The email verification token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<AuthResult> VerifyEmailAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resends the email verification link.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<AuthResult> ResendVerificationEmailAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes authentication tokens using a refresh token.
    /// Implements token rotation - old refresh token is revoked.
    /// </summary>
    /// <param name="refreshToken">The refresh token to use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>New token pair or error if refresh token is invalid.</returns>
    Task<AuthResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of an authentication operation.
/// </summary>
public sealed record AuthResult
{
    /// <summary>
    /// Whether the operation was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Whether the operation failed.
    /// </summary>
    public bool IsFailure => !Success;

    /// <summary>
    /// Error code if the operation failed.
    /// </summary>
    public AuthErrorCode? ErrorCode { get; init; }

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Token pair if authentication was successful.
    /// </summary>
    public TokenPair? Tokens { get; init; }

    /// <summary>
    /// The authenticated user's ID.
    /// </summary>
    public UserId? UserId { get; init; }

    /// <summary>
    /// The authenticated user's email.
    /// </summary>
    public Email? Email { get; init; }

    /// <summary>
    /// The authenticated user object (for accessing FamilyId, EmailVerified, etc.).
    /// </summary>
    public AuthenticatedUser? User { get; init; }

    /// <summary>
    /// Convenience property for access token.
    /// </summary>
    public string? AccessToken => Tokens?.AccessToken;

    /// <summary>
    /// Convenience property for refresh token.
    /// </summary>
    public string? RefreshToken => Tokens?.RefreshToken;

    /// <summary>
    /// Creates a successful result with tokens and user details.
    /// </summary>
    public static AuthResult Succeeded(TokenPair tokens, UserId userId, Email email, AuthenticatedUser user) => new()
    {
        Success = true,
        Tokens = tokens,
        UserId = userId,
        Email = email,
        User = user
    };

    /// <summary>
    /// Creates a successful result with tokens.
    /// </summary>
    public static AuthResult Succeeded(TokenPair tokens, UserId userId) => new()
    {
        Success = true,
        Tokens = tokens,
        UserId = userId
    };

    /// <summary>
    /// Creates a successful result without tokens.
    /// </summary>
    public static AuthResult Succeeded() => new() { Success = true };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static AuthResult Failed(AuthErrorCode errorCode, string? message = null) => new()
    {
        Success = false,
        ErrorCode = errorCode,
        ErrorMessage = message ?? GetDefaultMessage(errorCode)
    };

    private static string GetDefaultMessage(AuthErrorCode errorCode) => errorCode switch
    {
        AuthErrorCode.INVALID_CREDENTIALS => "Invalid email or password.",
        AuthErrorCode.ACCOUNT_LOCKED => "Account is temporarily locked due to too many failed login attempts.",
        AuthErrorCode.EMAIL_NOT_VERIFIED => "Please verify your email address before logging in.",
        AuthErrorCode.EMAIL_ALREADY_EXISTS => "An account with this email already exists.",
        AuthErrorCode.INVALID_TOKEN => "The token is invalid or has expired.",
        AuthErrorCode.INVALID_CODE => "The code is invalid or has expired.",
        AuthErrorCode.PASSWORD_TOO_WEAK => "Password does not meet security requirements.",
        AuthErrorCode.PASSWORD_MISMATCH => "Current password is incorrect.",
        AuthErrorCode.USER_NOT_FOUND => "User not found.",
        AuthErrorCode.EMAIL_ALREADY_VERIFIED => "Email is already verified.",
        _ => "An error occurred during authentication."
    };
}

/// <summary>
/// Authenticated user details returned from login/register.
/// </summary>
public sealed record AuthenticatedUser
{
    /// <summary>The user's family ID (if member of a family).</summary>
    public FamilyId? FamilyId { get; init; }

    /// <summary>Whether the user's email is verified.</summary>
    public bool EmailVerified { get; init; }
}

/// <summary>
/// Error codes for authentication operations.
/// </summary>
public enum AuthErrorCode
{
    /// <summary>Invalid email or password.</summary>
    INVALID_CREDENTIALS,

    /// <summary>Account is locked due to too many failed attempts.</summary>
    ACCOUNT_LOCKED,

    /// <summary>Email address has not been verified.</summary>
    EMAIL_NOT_VERIFIED,

    /// <summary>An account with this email already exists.</summary>
    EMAIL_ALREADY_EXISTS,

    /// <summary>The token is invalid or expired.</summary>
    INVALID_TOKEN,

    /// <summary>The reset code is invalid or expired.</summary>
    INVALID_CODE,

    /// <summary>Password does not meet requirements.</summary>
    PASSWORD_TOO_WEAK,

    /// <summary>Current password is incorrect.</summary>
    PASSWORD_MISMATCH,

    /// <summary>User not found.</summary>
    USER_NOT_FOUND,

    /// <summary>Email is already verified.</summary>
    EMAIL_ALREADY_VERIFIED,

    /// <summary>An unknown error occurred.</summary>
    UNKNOWN
}

/// <summary>
/// Request to register a new user.
/// </summary>
public sealed record RegisterRequest
{
    /// <summary>User's email address.</summary>
    public required string Email { get; init; }

    /// <summary>User's password.</summary>
    public required string Password { get; init; }

    /// <summary>Optional device information.</summary>
    public string? DeviceInfo { get; init; }

    /// <summary>Optional IP address.</summary>
    public string? IpAddress { get; init; }
}

/// <summary>
/// Request to login.
/// </summary>
public sealed record LoginRequest
{
    /// <summary>User's email address.</summary>
    public required string Email { get; init; }

    /// <summary>User's password.</summary>
    public required string Password { get; init; }

    /// <summary>Optional device information.</summary>
    public string? DeviceInfo { get; init; }

    /// <summary>Optional IP address.</summary>
    public string? IpAddress { get; init; }
}

/// <summary>
/// Request to change password.
/// </summary>
public sealed record ChangePasswordRequest
{
    /// <summary>The user's ID.</summary>
    public required UserId UserId { get; init; }

    /// <summary>Current password for verification.</summary>
    public required string CurrentPassword { get; init; }

    /// <summary>New password.</summary>
    public required string NewPassword { get; init; }
}

/// <summary>
/// Request to initiate password reset.
/// </summary>
public sealed record RequestPasswordResetRequest
{
    /// <summary>User's email address.</summary>
    public required string Email { get; init; }

    /// <summary>Whether to send a 6-digit code instead of a link (for mobile).</summary>
    public bool UseMobileCode { get; init; }
}

/// <summary>
/// Request to reset password with token (web flow).
/// </summary>
public sealed record ResetPasswordWithTokenRequest
{
    /// <summary>Password reset token from email.</summary>
    public required string Token { get; init; }

    /// <summary>New password.</summary>
    public required string NewPassword { get; init; }
}

/// <summary>
/// Request to reset password with code (mobile flow).
/// </summary>
public sealed record ResetPasswordWithCodeRequest
{
    /// <summary>User's email address.</summary>
    public required string Email { get; init; }

    /// <summary>6-digit reset code.</summary>
    public required string Code { get; init; }

    /// <summary>New password.</summary>
    public required string NewPassword { get; init; }
}
