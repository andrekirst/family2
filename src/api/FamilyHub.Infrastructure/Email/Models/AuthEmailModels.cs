namespace FamilyHub.Infrastructure.Email.Models;

/// <summary>
/// Model for email verification emails.
/// </summary>
public sealed record EmailVerificationModel
{
    /// <summary>
    /// The user's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// The verification URL to click.
    /// </summary>
    public required string VerificationUrl { get; init; }

    /// <summary>
    /// How long the verification link is valid.
    /// </summary>
    public required string ExpirationTime { get; init; }
}

/// <summary>
/// Model for password reset link emails (web flow).
/// </summary>
public sealed record PasswordResetLinkModel
{
    /// <summary>
    /// The user's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// The password reset URL to click.
    /// </summary>
    public required string ResetUrl { get; init; }

    /// <summary>
    /// How long the reset link is valid.
    /// </summary>
    public required string ExpirationTime { get; init; }
}

/// <summary>
/// Model for password reset code emails (mobile flow).
/// </summary>
public sealed record PasswordResetCodeModel
{
    /// <summary>
    /// The user's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// The 6-digit reset code.
    /// </summary>
    public required string ResetCode { get; init; }

    /// <summary>
    /// How long the reset code is valid.
    /// </summary>
    public required string ExpirationTime { get; init; }
}

/// <summary>
/// Model for welcome emails after registration.
/// </summary>
public sealed record WelcomeEmailModel
{
    /// <summary>
    /// The user's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// URL to the application.
    /// </summary>
    public required string AppUrl { get; init; }
}

/// <summary>
/// Model for password changed alert emails.
/// </summary>
public sealed record PasswordChangedAlertModel
{
    /// <summary>
    /// The user's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// When the password was changed.
    /// </summary>
    public required string ChangedAt { get; init; }

    /// <summary>
    /// URL to reset password if this wasn't the user.
    /// </summary>
    public required string ResetPasswordUrl { get; init; }
}
