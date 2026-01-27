namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;

/// <summary>
/// GraphQL input for user login.
/// Authenticates user with email and password.
/// </summary>
public sealed record LoginInput
{
    /// <summary>
    /// User's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User's password.
    /// </summary>
    public required string Password { get; init; }
}
