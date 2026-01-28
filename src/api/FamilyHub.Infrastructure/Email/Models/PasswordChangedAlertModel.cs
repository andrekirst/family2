namespace FamilyHub.Infrastructure.Email.Models;

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