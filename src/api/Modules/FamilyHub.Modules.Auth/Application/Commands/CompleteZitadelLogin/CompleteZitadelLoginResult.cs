using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Commands.CompleteZitadelLogin;

/// <summary>
/// Result of completing Zitadel OAuth login.
/// Contains user information and authentication tokens.
/// </summary>
public sealed record CompleteZitadelLoginResult : ITimestampable
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
    /// User's family ID
    /// </summary>
    public required FamilyId FamilyId { get; init; }

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
    public required DateTime CreatedAt { get; set; }

    /// <summary>
    /// User account last update timestamp
    /// </summary>
    public required DateTime UpdatedAt { get; set; }
}
