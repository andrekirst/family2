using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain;

public class ShareLinkTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var familyId = FamilyId.New();
        var createdBy = UserId.New();
        var resourceId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var link = ShareLink.Create(
            ShareResourceType.File,
            resourceId,
            familyId,
            createdBy,
            expiresAt,
            null,
            null);

        link.ResourceType.Should().Be(ShareResourceType.File);
        link.ResourceId.Should().Be(resourceId);
        link.FamilyId.Should().Be(familyId);
        link.CreatedBy.Should().Be(createdBy);
        link.ExpiresAt.Should().Be(expiresAt);
        link.PasswordHash.Should().BeNull();
        link.MaxDownloads.Should().BeNull();
        link.DownloadCount.Should().Be(0);
        link.IsRevoked.Should().BeFalse();
        link.Token.Should().NotBeNullOrEmpty();
        link.Token.Length.Should().BeGreaterThan(20);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueTokens()
    {
        var link1 = ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(), null, null, null);
        var link2 = ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(), null, null, null);

        link1.Token.Should().NotBe(link2.Token);
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        var link = ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(), null, null, null);

        link.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void IsAccessible_WhenNotExpiredNotRevoked_ShouldBeTrue()
    {
        var link = ShareLink.Create(
            ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(),
            DateTime.UtcNow.AddDays(7), null, null);

        link.IsAccessible.Should().BeTrue();
    }

    [Fact]
    public void IsAccessible_WhenNoExpiration_ShouldBeTrue()
    {
        var link = ShareLink.Create(
            ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(),
            null, null, null);

        link.IsAccessible.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenPastExpiration_ShouldBeTrue()
    {
        var link = ShareLink.Create(
            ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(),
            DateTime.UtcNow.AddHours(-1), null, null);

        link.IsExpired.Should().BeTrue();
        link.IsAccessible.Should().BeFalse();
    }

    [Fact]
    public void Revoke_ShouldSetIsRevoked()
    {
        var link = ShareLink.Create(
            ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(),
            null, null, null);

        link.Revoke(UserId.New());

        link.IsRevoked.Should().BeTrue();
        link.IsAccessible.Should().BeFalse();
        link.DomainEvents.Should().HaveCount(2); // Created + Revoked
    }

    [Fact]
    public void IncrementDownloadCount_ShouldIncrement()
    {
        var link = ShareLink.Create(
            ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(),
            null, null, 5);

        link.IncrementDownloadCount();
        link.IncrementDownloadCount();

        link.DownloadCount.Should().Be(2);
    }

    [Fact]
    public void IsDownloadLimitReached_WhenLimitReached_ShouldBeTrue()
    {
        var link = ShareLink.Create(
            ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(),
            null, null, 2);

        link.IncrementDownloadCount();
        link.IncrementDownloadCount();

        link.IsDownloadLimitReached.Should().BeTrue();
        link.IsAccessible.Should().BeFalse();
    }

    [Fact]
    public void HasPassword_WhenPasswordSet_ShouldBeTrue()
    {
        var link = ShareLink.Create(
            ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(),
            null, "hashed-password", null);

        link.HasPassword.Should().BeTrue();
    }

    [Fact]
    public void HasPassword_WhenNoPassword_ShouldBeFalse()
    {
        var link = ShareLink.Create(
            ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(),
            null, null, null);

        link.HasPassword.Should().BeFalse();
    }

    [Fact]
    public void Token_ShouldBeUrlSafe()
    {
        var link = ShareLink.Create(
            ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(),
            null, null, null);

        link.Token.Should().NotContain("+");
        link.Token.Should().NotContain("/");
        link.Token.Should().NotContain("=");
    }
}
