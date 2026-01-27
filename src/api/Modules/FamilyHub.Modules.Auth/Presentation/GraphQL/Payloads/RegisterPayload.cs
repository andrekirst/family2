namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for user registration result.
/// </summary>
public sealed class RegisterPayload
{
    /// <summary>
    /// The ID of the newly created user.
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// User's email address.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Indicates whether email verification is required before login.
    /// </summary>
    public bool EmailVerificationRequired { get; init; }

    /// <summary>
    /// JWT access token (null if email verification required first).
    /// </summary>
    public string? AccessToken { get; init; }

    /// <summary>
    /// Refresh token (null if email verification required first).
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>
    /// Errors that occurred during registration.
    /// </summary>
    public IReadOnlyList<PayloadError>? Errors { get; init; }
}
