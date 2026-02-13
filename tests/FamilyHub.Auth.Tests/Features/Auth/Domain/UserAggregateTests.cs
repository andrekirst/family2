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
