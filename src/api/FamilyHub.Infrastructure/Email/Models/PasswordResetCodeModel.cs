namespace FamilyHub.Infrastructure.Email.Models;

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