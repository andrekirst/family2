using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Events;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;

namespace FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;

public sealed class GoogleAccountLink : AggregateRoot<GoogleAccountLinkId>
{
#pragma warning disable CS8618 // EF Core requires parameterless constructor
    private GoogleAccountLink() { }
#pragma warning restore CS8618

    public UserId UserId { get; private set; }
    public GoogleAccountId GoogleAccountId { get; private set; }
    public Email GoogleEmail { get; private set; }
    public EncryptedToken EncryptedAccessToken { get; private set; }
    public EncryptedToken EncryptedRefreshToken { get; private set; }
    public DateTime AccessTokenExpiresAt { get; private set; }
    public GoogleScopes GrantedScopes { get; private set; }
    public GoogleLinkStatus Status { get; private set; }
    public DateTime? LastSyncAt { get; private set; }
    public string? LastError { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static GoogleAccountLink Create(
        UserId userId,
        GoogleAccountId googleAccountId,
        Email googleEmail,
        EncryptedToken encryptedAccessToken,
        EncryptedToken encryptedRefreshToken,
        DateTime accessTokenExpiresAt,
        GoogleScopes grantedScopes,
        DateTimeOffset utcNow)
    {
        var link = new GoogleAccountLink
        {
            Id = GoogleAccountLinkId.New(),
            UserId = userId,
            GoogleAccountId = googleAccountId,
            GoogleEmail = googleEmail,
            EncryptedAccessToken = encryptedAccessToken,
            EncryptedRefreshToken = encryptedRefreshToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            GrantedScopes = grantedScopes,
            Status = GoogleLinkStatus.Active,
            CreatedAt = utcNow.UtcDateTime,
            UpdatedAt = utcNow.UtcDateTime
        };

        link.RaiseDomainEvent(new GoogleAccountLinkedEvent(
            link.Id, userId, googleAccountId, grantedScopes));

        return link;
    }

    public void RefreshAccessToken(
        EncryptedToken newEncryptedAccessToken,
        DateTime newExpiresAt,
        DateTimeOffset utcNow)
    {
        var now = utcNow;
        EncryptedAccessToken = newEncryptedAccessToken;
        AccessTokenExpiresAt = newExpiresAt;
        Status = GoogleLinkStatus.Active;
        LastError = null;
        UpdatedAt = now.UtcDateTime;

        RaiseDomainEvent(new GoogleTokenRefreshedEvent(Id, UserId, newExpiresAt));
    }

    public void MarkRefreshFailed(string error, DateTimeOffset utcNow)
    {
        var now = utcNow;
        Status = GoogleLinkStatus.Error;
        LastError = error;
        UpdatedAt = now.UtcDateTime;

        RaiseDomainEvent(new GoogleTokenRefreshFailedEvent(Id, UserId, error));
    }

    public void MarkRevoked(DateTimeOffset utcNow)
    {
        var now = utcNow;
        Status = GoogleLinkStatus.Revoked;
        UpdatedAt = now.UtcDateTime;

        RaiseDomainEvent(new GoogleAccountUnlinkedEvent(Id, UserId, GoogleAccountId));
    }

    public void RecordSync(DateTimeOffset utcNow)
    {
        var now = utcNow;
        LastSyncAt = now.UtcDateTime;
        UpdatedAt = now.UtcDateTime;
    }

    public bool IsAccessTokenExpired(DateTimeOffset utcNow) =>
        AccessTokenExpiresAt <= utcNow.UtcDateTime;

    public bool IsAccessTokenExpiringSoon(TimeSpan threshold, DateTimeOffset utcNow) =>
        AccessTokenExpiresAt <= utcNow.UtcDateTime.Add(threshold);
}
