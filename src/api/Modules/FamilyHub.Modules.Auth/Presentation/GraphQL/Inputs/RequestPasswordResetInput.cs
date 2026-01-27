namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;

/// <summary>
/// GraphQL input for requesting a password reset.
/// Sends a password reset link or code to the user's email.
/// </summary>
public sealed record RequestPasswordResetInput
{
    /// <summary>
    /// Email address of the account to reset.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// If true, sends a 6-digit code instead of a link.
    /// Useful for mobile apps where opening links is inconvenient.
    /// Default is false (sends link).
    /// </summary>
    public bool UseMobileCode { get; init; }
}
