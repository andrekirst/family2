using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain;

/// <summary>
/// Represents a refresh token for JWT token rotation.
/// Refresh tokens are stored as SHA256 hashes for security.
/// Supports token rotation: when a token is used, it's revoked and a new one is issued.
/// </summary>
public sealed class RefreshToken : Entity<RefreshTokenId>
{
    /// <summary>
    /// The user this refresh token belongs to.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// SHA256 hash of the actual refresh token.
    /// We never store the raw token, only its hash.
    /// </summary>
    public string TokenHash { get; private set; } = string.Empty;

    /// <summary>
    /// When this refresh token expires.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Device information (User-Agent or custom device identifier).
    /// Used for session management UI.
    /// </summary>
    public string? DeviceInfo { get; private set; }

    /// <summary>
    /// IP address from which the token was issued.
    /// Stored for audit purposes.
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// Whether this token has been revoked.
    /// </summary>
    public bool IsRevoked { get; private set; }

    /// <summary>
    /// When this token was revoked (null if not revoked).
    /// </summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>
    /// If this token was rotated, this points to the new token.
    /// Used for detecting token reuse attacks.
    /// </summary>
    public RefreshTokenId? ReplacedByTokenId { get; private set; }

    /// <summary>
    /// Whether this token is still valid (not revoked and not expired).
    /// </summary>
    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;

    /// <summary>
    /// Whether this token has expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    // Private constructor for EF Core
    private RefreshToken() : base(RefreshTokenId.New())
    {
        UserId = UserId.From(Guid.Empty);
    }

    private RefreshToken(
        RefreshTokenId id,
        UserId userId,
        string tokenHash,
        DateTime expiresAt,
        string? deviceInfo,
        string? ipAddress) : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        DeviceInfo = deviceInfo;
        IpAddress = ipAddress;
        IsRevoked = false;
    }

    /// <summary>
    /// Creates a new refresh token.
    /// </summary>
    /// <param name="userId">The user this token belongs to.</param>
    /// <param name="tokenHash">SHA256 hash of the raw token.</param>
    /// <param name="lifetime">How long the token is valid.</param>
    /// <param name="deviceInfo">Optional device information.</param>
    /// <param name="ipAddress">Optional IP address.</param>
    public static RefreshToken Create(
        UserId userId,
        string tokenHash,
        TimeSpan lifetime,
        string? deviceInfo = null,
        string? ipAddress = null)
    {
        return new RefreshToken(
            RefreshTokenId.New(),
            userId,
            tokenHash,
            DateTime.UtcNow.Add(lifetime),
            deviceInfo,
            ipAddress);
    }

    /// <summary>
    /// Revokes this refresh token.
    /// </summary>
    /// <param name="replacedByTokenId">If this token is being rotated, the ID of the new token.</param>
    public void Revoke(RefreshTokenId? replacedByTokenId = null)
    {
        if (IsRevoked)
        {
            return;
        }

        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        ReplacedByTokenId = replacedByTokenId;
    }
}
