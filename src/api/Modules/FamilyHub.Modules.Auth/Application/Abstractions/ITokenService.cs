using FamilyHub.Modules.Auth.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Abstractions;

/// <summary>
/// Service for JWT token generation, validation, and refresh token management.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates an access token and refresh token pair for the user.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <param name="deviceInfo">Optional device information for the session.</param>
    /// <param name="ipAddress">Optional IP address of the client.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A token pair containing access and refresh tokens.</returns>
    Task<TokenPair> GenerateTokenPairAsync(
        User user,
        string? deviceInfo = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// Implements refresh token rotation (old token is invalidated).
    /// </summary>
    /// <param name="refreshToken">The refresh token string.</param>
    /// <param name="ipAddress">Optional IP address of the client.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A refresh result with new tokens and user ID, or null if the refresh token is invalid.</returns>
    Task<RefreshResult?> RefreshTokensAsync(
        string refreshToken,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a specific refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to revoke.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the token was revoked, false if not found.</returns>
    Task<bool> RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all refresh tokens for a user (logout from all devices).
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of tokens revoked.</returns>
    Task<int> RevokeAllUserTokensAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active sessions (non-revoked refresh tokens) for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of active sessions.</returns>
    Task<IReadOnlyList<ActiveSession>> GetActiveSessionsAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a JWT access token and extracts the claims.
    /// </summary>
    /// <param name="accessToken">The JWT access token.</param>
    /// <returns>The claims principal if valid, null otherwise.</returns>
    System.Security.Claims.ClaimsPrincipal? ValidateAccessToken(string accessToken);
}

/// <summary>
/// Result of a token refresh operation.
/// </summary>
public sealed record RefreshResult
{
    /// <summary>
    /// The user ID associated with the refresh token.
    /// </summary>
    public required UserId UserId { get; init; }

    /// <summary>
    /// The new token pair.
    /// </summary>
    public required TokenPair Tokens { get; init; }
}

/// <summary>
/// Represents an access token and refresh token pair.
/// </summary>
public sealed record TokenPair
{
    /// <summary>
    /// The JWT access token.
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// The opaque refresh token.
    /// </summary>
    public required string RefreshToken { get; init; }

    /// <summary>
    /// When the access token expires.
    /// </summary>
    public required DateTime AccessTokenExpiresAt { get; init; }

    /// <summary>
    /// When the refresh token expires.
    /// </summary>
    public required DateTime RefreshTokenExpiresAt { get; init; }

    /// <summary>
    /// Token type (always "Bearer").
    /// </summary>
    public string TokenType => "Bearer";
}

/// <summary>
/// Represents an active user session.
/// </summary>
public sealed record ActiveSession
{
    /// <summary>
    /// The session/refresh token ID.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// Device information for the session.
    /// </summary>
    public string? DeviceInfo { get; init; }

    /// <summary>
    /// IP address from which the session was created.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// When the session was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the session expires.
    /// </summary>
    public required DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Whether this is the current session.
    /// </summary>
    public bool IsCurrent { get; init; }
}
