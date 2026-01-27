using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Commands.Login;

/// <summary>
/// Command to authenticate a user with email and password.
/// Returns JWT access token and refresh token on success.
/// UNAUTHENTICATED: No authorization requirements (public login).
/// </summary>
public sealed record LoginCommand(
    Email Email,
    string Password
) : ICommand<FamilyHub.SharedKernel.Domain.Result<LoginResult>>;

/// <summary>
/// Result of successful user login.
/// </summary>
public sealed record LoginResult
{
    /// <summary>
    /// The authenticated user's ID.
    /// </summary>
    public required UserId UserId { get; init; }

    /// <summary>
    /// User's email address.
    /// </summary>
    public required Email Email { get; init; }

    /// <summary>
    /// JWT access token for API authentication.
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// Refresh token for obtaining new access tokens.
    /// </summary>
    public required string RefreshToken { get; init; }

    /// <summary>
    /// Access token expiration time in seconds.
    /// </summary>
    public required int ExpiresIn { get; init; }

    /// <summary>
    /// User's family ID (if member of a family).
    /// </summary>
    public FamilyId? FamilyId { get; init; }

    /// <summary>
    /// Whether the user's email is verified.
    /// </summary>
    public bool EmailVerified { get; init; }
}
