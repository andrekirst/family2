namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;

/// <summary>
/// GraphQL input for user registration.
/// Creates a new user account with email verification requirement.
/// </summary>
public sealed record RegisterInput
{
    /// <summary>
    /// User's email address. Must be unique in the system.
    /// Will be used for email verification and password reset.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User's password. Must meet password policy requirements:
    /// - Minimum 12 characters
    /// - At least one uppercase letter
    /// - At least one lowercase letter
    /// - At least one digit
    /// - At least one special character
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// Password confirmation. Must match the password field.
    /// </summary>
    public required string ConfirmPassword { get; init; }
}
