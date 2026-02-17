using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.Events;
using FluentAssertions;

namespace FamilyHub.Auth.Tests.Features.Auth.Domain;

public class UserAggregateTests
{
    [Fact]
    public void Register_ShouldCreateUserWithValidData()
    {
        // Arrange
        var email = Email.From("test@example.com");
        var name = UserName.From("Test User");
        var externalId = ExternalUserId.From("keycloak-123");

        // Act
        var user = User.Register(email, name, externalId, emailVerified: true);

        // Assert
        user.Should().NotBeNull();
        user.Id.Value.Should().NotBe(Guid.Empty);
        user.Email.Should().Be(email);
        user.Name.Should().Be(name);
        user.ExternalUserId.Should().Be(externalId);
        user.EmailVerified.Should().BeTrue();
        user.IsActive.Should().BeTrue();
        user.FamilyId.Should().BeNull();
        user.ExternalProvider.Should().Be("KEYCLOAK");
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Register_ShouldRaiseUserRegisteredEvent()
    {
        // Arrange
        var email = Email.From("test@example.com");
        var name = UserName.From("Test User");
        var externalId = ExternalUserId.From("keycloak-123");

        // Act
        var user = User.Register(email, name, externalId, emailVerified: true);

        // Assert
        user.DomainEvents.Should().HaveCount(1);
        var domainEvent = user.DomainEvents.First();
        domainEvent.Should().BeOfType<UserRegisteredEvent>();

        var userRegisteredEvent = (UserRegisteredEvent)domainEvent;
        userRegisteredEvent.UserId.Should().Be(user.Id);
        userRegisteredEvent.Email.Should().Be(email);
        userRegisteredEvent.Name.Should().Be(name);
        userRegisteredEvent.EmailVerified.Should().BeTrue();
    }

    [Fact]
    public void UpdateLastLogin_ShouldUpdateTimestamp()
    {
        // Arrange
        var user = CreateTestUser();
        var loginTime = DateTime.UtcNow.AddMinutes(5);

        // Act
        user.UpdateLastLogin(loginTime);

        // Assert
        user.LastLoginAt.Should().Be(loginTime);
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void UpdateProfile_ShouldUpdateUserInfo()
    {
        // Arrange
        var user = CreateTestUser();
        var newEmail = Email.From("updated@example.com");
        var newName = UserName.From("Updated Name");

        // Act
        user.UpdateProfile(newEmail, newName, emailVerified: true);

        // Assert
        user.Email.Should().Be(newEmail);
        user.Name.Should().Be(newName);
        user.EmailVerified.Should().BeTrue();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void AssignToFamily_ShouldAssignUserAndRaiseEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var familyId = FamilyId.New();

        // Act
        user.AssignToFamily(familyId);

        // Assert
        user.FamilyId.Should().Be(familyId);
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

        // Verify event raised (1 event: UserFamilyAssigned - UserRegistered was cleared in helper)
        user.DomainEvents.Should().HaveCount(1);
        var familyAssignedEvent = user.DomainEvents.OfType<UserFamilyAssignedEvent>().FirstOrDefault();
        familyAssignedEvent.Should().NotBeNull();
        familyAssignedEvent!.UserId.Should().Be(user.Id);
        familyAssignedEvent.FamilyId.Should().Be(familyId);
    }

    [Fact]
    public void AssignToFamily_WhenAlreadyAssigned_ShouldThrowDomainException()
    {
        // Arrange
        var user = CreateTestUser();
        var familyId1 = FamilyId.New();
        var familyId2 = FamilyId.New();
        user.AssignToFamily(familyId1);

        // Act & Assert
        var act = () => user.AssignToFamily(familyId2);
        act.Should().Throw<DomainException>()
            .WithMessage("User is already assigned to a family");
    }

    [Fact]
    public void RemoveFromFamily_ShouldUnassignUserAndRaiseEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var familyId = FamilyId.New();
        user.AssignToFamily(familyId);
        user.ClearDomainEvents(); // Clear previous events

        // Act
        user.RemoveFromFamily();

        // Assert
        user.FamilyId.Should().BeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

        user.DomainEvents.Should().HaveCount(1);
        var familyRemovedEvent = user.DomainEvents.OfType<UserFamilyRemovedEvent>().FirstOrDefault();
        familyRemovedEvent.Should().NotBeNull();
        familyRemovedEvent!.UserId.Should().Be(user.Id);
        familyRemovedEvent.PreviousFamilyId.Should().Be(familyId);
    }

    [Fact]
    public void RemoveFromFamily_WhenNotAssigned_ShouldThrowDomainException()
    {
        // Arrange
        var user = CreateTestUser();

        // Act & Assert
        var act = () => user.RemoveFromFamily();
        act.Should().Throw<DomainException>()
            .WithMessage("User is not assigned to any family");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Reactivate_ShouldSetIsActiveTrue()
    {
        // Arrange
        var user = CreateTestUser();
        user.Deactivate();

        // Act
        user.Reactivate();

        // Assert
        user.IsActive.Should().BeTrue();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void SetAvatar_ShouldSetAvatarIdAndRaiseEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var avatarId = AvatarId.New();

        // Act
        user.SetAvatar(avatarId);

        // Assert
        user.AvatarId.Should().Be(avatarId);
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

        user.DomainEvents.Should().HaveCount(1);
        var avatarChangedEvent = user.DomainEvents.OfType<UserAvatarChangedEvent>().FirstOrDefault();
        avatarChangedEvent.Should().NotBeNull();
        avatarChangedEvent!.UserId.Should().Be(user.Id);
        avatarChangedEvent.NewAvatarId.Should().Be(avatarId);
        avatarChangedEvent.PreviousAvatarId.Should().BeNull();
    }

    [Fact]
    public void SetAvatar_ShouldIncludePreviousAvatarId_WhenReplacing()
    {
        // Arrange
        var user = CreateTestUser();
        var oldAvatarId = AvatarId.New();
        var newAvatarId = AvatarId.New();
        user.SetAvatar(oldAvatarId);
        user.ClearDomainEvents();

        // Act
        user.SetAvatar(newAvatarId);

        // Assert
        user.AvatarId.Should().Be(newAvatarId);
        var avatarChangedEvent = user.DomainEvents.OfType<UserAvatarChangedEvent>().Single();
        avatarChangedEvent.PreviousAvatarId.Should().Be(oldAvatarId);
        avatarChangedEvent.NewAvatarId.Should().Be(newAvatarId);
    }

    [Fact]
    public void RemoveAvatar_ShouldClearAvatarIdAndRaiseEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var avatarId = AvatarId.New();
        user.SetAvatar(avatarId);
        user.ClearDomainEvents();

        // Act
        user.RemoveAvatar();

        // Assert
        user.AvatarId.Should().BeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

        user.DomainEvents.Should().HaveCount(1);
        var avatarRemovedEvent = user.DomainEvents.OfType<UserAvatarRemovedEvent>().FirstOrDefault();
        avatarRemovedEvent.Should().NotBeNull();
        avatarRemovedEvent!.UserId.Should().Be(user.Id);
        avatarRemovedEvent.RemovedAvatarId.Should().Be(avatarId);
    }

    [Fact]
    public void RemoveAvatar_ShouldNotRaiseEvent_WhenNoAvatarSet()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        user.RemoveAvatar();

        // Assert
        user.AvatarId.Should().BeNull();
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateLocale_ShouldSetPreferredLocale()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        user.UpdateLocale("de");

        // Assert
        user.PreferredLocale.Should().Be("de");
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Register_ShouldDefaultPreferredLocaleToEnglish()
    {
        // Arrange & Act
        var user = CreateTestUser();

        // Assert
        user.PreferredLocale.Should().Be("en");
    }

    private static User CreateTestUser()
    {
        var email = Email.From("test@example.com");
        var name = UserName.From("Test User");
        var externalId = ExternalUserId.From("test-external-id");

        var user = User.Register(email, name, externalId, emailVerified: false);
        user.ClearDomainEvents(); // Clear registration event for cleaner tests

        return user;
    }
}
