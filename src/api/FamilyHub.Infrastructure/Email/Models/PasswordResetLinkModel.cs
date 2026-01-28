namespace FamilyHub.Infrastructure.Email.Models;

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