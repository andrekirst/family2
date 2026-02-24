using FluentAssertions;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Events;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;

namespace FamilyHub.GoogleIntegration.Tests.Domain;

public class GoogleAccountLinkAggregateTests
{
    private static GoogleAccountLink CreateTestLink()
    {
        return GoogleAccountLink.Create(
            UserId.New(),
            GoogleAccountId.From("google-sub-123"),
            Email.From("test@gmail.com"),
            EncryptedToken.From("encrypted-access"),
            EncryptedToken.From("encrypted-refresh"),
            DateTime.UtcNow.AddHours(1),
            GoogleScopes.From("openid email https://www.googleapis.com/auth/calendar.readonly"));
    }

    [Fact]
    public void Create_ShouldSetProperties()
    {
        var userId = UserId.New();
        var googleId = GoogleAccountId.From("sub-456");
        var email = Email.From("user@gmail.com");

        var link = GoogleAccountLink.Create(
            userId, googleId, email,
            EncryptedToken.From("access"), EncryptedToken.From("refresh"),
            DateTime.UtcNow.AddHours(1),
            GoogleScopes.From("openid email"));

        link.UserId.Should().Be(userId);
        link.GoogleAccountId.Should().Be(googleId);
        link.GoogleEmail.Should().Be(email);
        link.Status.Should().Be(GoogleLinkStatus.Active);
        link.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ShouldRaiseGoogleAccountLinkedEvent()
    {
        var link = CreateTestLink();

        link.DomainEvents.Should().HaveCount(1);
        var domainEvent = link.DomainEvents.First();
        domainEvent.Should().BeOfType<GoogleAccountLinkedEvent>();

        var linkedEvent = (GoogleAccountLinkedEvent)domainEvent;
        linkedEvent.LinkId.Should().Be(link.Id);
        linkedEvent.UserId.Should().Be(link.UserId);
        linkedEvent.GoogleAccountId.Should().Be(link.GoogleAccountId);
    }

    [Fact]
    public void RefreshAccessToken_ShouldUpdateTokenAndExpiry()
    {
        var link = CreateTestLink();
        link.ClearDomainEvents();

        var newToken = EncryptedToken.From("new-encrypted-access");
        var newExpiry = DateTime.UtcNow.AddHours(2);

        link.RefreshAccessToken(newToken, newExpiry);

        link.EncryptedAccessToken.Should().Be(newToken);
        link.AccessTokenExpiresAt.Should().Be(newExpiry);
        link.Status.Should().Be(GoogleLinkStatus.Active);
        link.LastError.Should().BeNull();
    }

    [Fact]
    public void RefreshAccessToken_ShouldRaiseGoogleTokenRefreshedEvent()
    {
        var link = CreateTestLink();
        link.ClearDomainEvents();

        var newExpiry = DateTime.UtcNow.AddHours(2);
        link.RefreshAccessToken(EncryptedToken.From("new"), newExpiry);

        link.DomainEvents.Should().HaveCount(1);
        var evt = link.DomainEvents.First().Should().BeOfType<GoogleTokenRefreshedEvent>().Subject;
        evt.NewExpiresAt.Should().Be(newExpiry);
    }

    [Fact]
    public void MarkRefreshFailed_ShouldSetErrorStatus()
    {
        var link = CreateTestLink();
        link.ClearDomainEvents();

        link.MarkRefreshFailed("Token expired permanently");

        link.Status.Should().Be(GoogleLinkStatus.Error);
        link.LastError.Should().Be("Token expired permanently");
    }

    [Fact]
    public void MarkRefreshFailed_ShouldRaiseFailedEvent()
    {
        var link = CreateTestLink();
        link.ClearDomainEvents();

        link.MarkRefreshFailed("error message");

        link.DomainEvents.Should().HaveCount(1);
        var evt = link.DomainEvents.First().Should().BeOfType<GoogleTokenRefreshFailedEvent>().Subject;
        evt.Error.Should().Be("error message");
    }

    [Fact]
    public void MarkRevoked_ShouldSetRevokedStatus()
    {
        var link = CreateTestLink();
        link.ClearDomainEvents();

        link.MarkRevoked();

        link.Status.Should().Be(GoogleLinkStatus.Revoked);
    }

    [Fact]
    public void MarkRevoked_ShouldRaiseUnlinkedEvent()
    {
        var link = CreateTestLink();
        link.ClearDomainEvents();

        link.MarkRevoked();

        link.DomainEvents.Should().HaveCount(1);
        link.DomainEvents.First().Should().BeOfType<GoogleAccountUnlinkedEvent>();
    }

    [Fact]
    public void IsAccessTokenExpired_ShouldReturnTrueWhenExpired()
    {
        var link = GoogleAccountLink.Create(
            UserId.New(),
            GoogleAccountId.From("sub"),
            Email.From("a@b.com"),
            EncryptedToken.From("a"), EncryptedToken.From("r"),
            DateTime.UtcNow.AddMinutes(-5),
            GoogleScopes.From("openid"));

        link.IsAccessTokenExpired().Should().BeTrue();
    }

    [Fact]
    public void IsAccessTokenExpired_ShouldReturnFalseWhenValid()
    {
        var link = CreateTestLink();
        link.IsAccessTokenExpired().Should().BeFalse();
    }

    [Fact]
    public void IsAccessTokenExpiringSoon_ShouldReturnTrueWithinThreshold()
    {
        var link = GoogleAccountLink.Create(
            UserId.New(),
            GoogleAccountId.From("sub"),
            Email.From("a@b.com"),
            EncryptedToken.From("a"), EncryptedToken.From("r"),
            DateTime.UtcNow.AddMinutes(3),
            GoogleScopes.From("openid"));

        link.IsAccessTokenExpiringSoon(TimeSpan.FromMinutes(5)).Should().BeTrue();
    }

    [Fact]
    public void RecordSync_ShouldUpdateLastSyncAt()
    {
        var link = CreateTestLink();

        link.RecordSync();

        link.LastSyncAt.Should().NotBeNull();
        link.LastSyncAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}
