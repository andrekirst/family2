namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL type representing a registered user.
/// </summary>
public sealed record UserType
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Whether the user's email has been verified.
    /// </summary>
    public required bool EmailVerified { get; init; }

    /// <summary>
    /// When the user account was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
