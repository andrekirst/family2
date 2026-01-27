namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for login result.
/// </summary>
public sealed class LoginPayload
{
    /// <summary>
    /// The authenticated user's ID.
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// User's email address.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// JWT access token.
    /// </summary>
    public string? AccessToken { get; init; }

    /// <summary>
    /// Refresh token for token rotation.
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>
    /// Access token expiration time in seconds.
    /// </summary>
    public int? ExpiresIn { get; init; }

    /// <summary>
    /// User's family ID (if member of a family).
    /// </summary>
    public Guid? FamilyId { get; init; }

    /// <summary>
    /// Whether the user's email is verified.
    /// </summary>
    public bool EmailVerified { get; init; }

    /// <summary>
    /// Errors that occurred during login.
    /// </summary>
    public IReadOnlyList<PayloadError>? Errors { get; init; }
}
