using FamilyHub.Api.Domain.Base;
using FamilyHub.Api.Domain.ValueObjects;

namespace FamilyHub.Api.Domain.Entities;

public class RefreshToken : Entity<RefreshTokenId>
{
    public UserId UserId { get; private set; }
    public string Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? DeviceInfo { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;
    public bool IsExpired => ExpiresAt <= DateTime.UtcNow;

    // Navigation
    public User? User { get; private set; }

    // EF Core constructor
    private RefreshToken() : base(RefreshTokenId.New())
    {
        Token = string.Empty;
    }

    private RefreshToken(RefreshTokenId id, UserId userId, string token, DateTime expiresAt, string? deviceInfo)
        : base(id)
    {
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        DeviceInfo = deviceInfo;
        CreatedAt = DateTime.UtcNow;
    }

    public static RefreshToken Create(UserId userId, string token, DateTime expiresAt, string? deviceInfo = null)
    {
        return new RefreshToken(RefreshTokenId.New(), userId, token, expiresAt, deviceInfo);
    }

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }
}
