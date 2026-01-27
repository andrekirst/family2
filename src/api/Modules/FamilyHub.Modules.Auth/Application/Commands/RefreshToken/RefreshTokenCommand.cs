using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Commands.RefreshToken;

/// <summary>
/// Command to refresh authentication tokens using a refresh token.
/// Implements token rotation: old refresh token is revoked, new pair issued.
/// UNAUTHENTICATED: Uses refresh token instead of JWT for auth.
/// </summary>
public sealed record RefreshTokenCommand(
    string RefreshToken
) : ICommand<FamilyHub.SharedKernel.Domain.Result<RefreshTokenResult>>;

/// <summary>
/// Result of successful token refresh.
/// </summary>
public sealed record RefreshTokenResult
{
    /// <summary>
    /// New JWT access token.
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// New refresh token (old one is revoked).
    /// </summary>
    public required string RefreshToken { get; init; }

    /// <summary>
    /// Access token expiration time in seconds.
    /// </summary>
    public required int ExpiresIn { get; init; }

    /// <summary>
    /// The user's ID.
    /// </summary>
    public required UserId UserId { get; init; }
}
