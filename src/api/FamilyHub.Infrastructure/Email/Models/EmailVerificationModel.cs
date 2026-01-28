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
