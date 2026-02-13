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
        GoogleScopes grantedScopes)
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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        link.RaiseDomainEvent(new GoogleAccountLinkedEvent(
            link.Id, userId, googleAccountId, grantedScopes));

        return link;
    }

    public void RefreshAccessToken(
        EncryptedToken newEncryptedAccessToken,
        DateTime newExpiresAt)
    {
        EncryptedAccessToken = newEncryptedAccessToken;
        AccessTokenExpiresAt = newExpiresAt;
        Status = GoogleLinkStatus.Active;
        LastError = null;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new GoogleTokenRefreshedEvent(Id, UserId, newExpiresAt));
    }

    public void MarkRefreshFailed(string error)
    {
        Status = GoogleLinkStatus.Error;
        LastError = error;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new GoogleTokenRefreshFailedEvent(Id, UserId, error));
    }

    public void MarkRevoked()
    {
        Status = GoogleLinkStatus.Revoked;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new GoogleAccountUnlinkedEvent(Id, UserId, GoogleAccountId));
    }

    public void RecordSync()
    {
        LastSyncAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsAccessTokenExpired() =>
        AccessTokenExpiresAt <= DateTime.UtcNow;

    public bool IsAccessTokenExpiringSoon(TimeSpan threshold) =>
        AccessTokenExpiresAt <= DateTime.UtcNow.Add(threshold);
}
