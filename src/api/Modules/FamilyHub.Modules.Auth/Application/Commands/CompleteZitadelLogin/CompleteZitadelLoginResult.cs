using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Commands.CompleteZitadelLogin;

/// <summary>
/// Result of completing Zitadel OAuth login.
/// </summary>
public sealed record CompleteZitadelLoginResult
{
    /// <summary>
    /// Internal user ID (our system's ID)
    /// </summary>
    public required UserId UserId { get; init; }

    /// <summary>
    /// User's email address
    /// </summary>
    public required Email Email { get; init; }

    /// <summary>
    /// Zitadel JWT access token (for API calls)
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// Access token expiration time
    /// </summary>
    public required DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Whether the email has been verified (always true for OAuth users)
    /// </summary>
    public required bool EmailVerified { get; init; }

    /// <summary>
    /// User account creation timestamp
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
