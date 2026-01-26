using FamilyHub.Modules.UserProfile.Domain.Events;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using UserProfileAggregate = FamilyHub.Modules.UserProfile.Domain.Aggregates.UserProfile;

namespace FamilyHub.Tests.Unit.UserProfile.Domain.Aggregates;

/// <summary>
/// Unit tests for the UserProfile aggregate root.
/// Tests domain logic, validation rules, and business invariants.
/// Uses FluentAssertions for readable, expressive test assertions.
/// </summary>
public class UserProfileTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidUserIdAndDisplayName_ShouldCreateProfile()
    {
        // Arrange
        var userId = UserId.New();
        var displayName = DisplayName.From("John Doe");

        // Act
        var profile = UserProfileAggregate.Create(userId, displayName);

        // Assert
        profile.Should().NotBeNull();
        profile.UserId.Should().Be(userId);
        profile.DisplayName.Should().Be(displayName);
        profile.Id.Value.Should().NotBe(Guid.Empty);
        profile.Birthday.Should().BeNull();
        profile.Pronouns.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldInitializeDefaultPreferences()
    {
        // Arrange
        var userId = UserId.New();
        var displayName = DisplayName.From("John Doe");

        // Act
        var profile = UserProfileAggregate.Create(userId, displayName);

        // Assert
        profile.Preferences.Should().NotBeNull();
        profile.Preferences.Language.Should().Be("en");
        profile.Preferences.Timezone.Should().Be("UTC");
        profile.Preferences.DateFormat.Should().Be("yyyy-MM-dd");
    }

    [Fact]
    public void Create_ShouldInitializeDefaultFieldVisibility()
    {
        // Arrange
        var userId = UserId.New();
        var displayName = DisplayName.From("John Doe");

        // Act
        var profile = UserProfileAggregate.Create(userId, displayName);

        // Assert
        profile.FieldVisibility.Should().NotBeNull();
        profile.FieldVisibility.BirthdayVisibility.Should().Be(VisibilityLevel.Family);
        profile.FieldVisibility.PronounsVisibility.Should().Be(VisibilityLevel.Family);
        profile.FieldVisibility.PreferencesVisibility.Should().Be(VisibilityLevel.Hidden);
    }

    [Fact]
    public void Create_ShouldRaiseUserProfileCreatedEvent()
    {
        // Arrange
        var userId = UserId.New();
        var displayName = DisplayName.From("John Doe");

        // Act
        var profile = UserProfileAggregate.Create(userId, displayName);

        // Assert
        profile.DomainEvents.Should().ContainSingle(e => e is UserProfileCreatedEvent);
        var createdEvent = profile.DomainEvents.OfType<UserProfileCreatedEvent>().Single();
        createdEvent.ProfileId.Should().Be(profile.Id);
        createdEvent.UserId.Should().Be(userId);
        createdEvent.DisplayName.Should().Be(displayName);
    }

    #endregion

    #region UpdateDisplayName Tests

    [Fact]
    public void UpdateDisplayName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var profile = CreateProfile();
        var newDisplayName = DisplayName.From("Jane Doe");

        // Act
        profile.UpdateDisplayName(newDisplayName);

        // Assert
        profile.DisplayName.Should().Be(newDisplayName);
    }

    [Fact]
    public void UpdateDisplayName_ShouldRaiseUserProfileUpdatedEvent()
    {
        // Arrange
        var profile = CreateProfile();
        profile.ClearDomainEvents(); // Clear creation event
        var newDisplayName = DisplayName.From("Jane Doe");

        // Act
        profile.UpdateDisplayName(newDisplayName);

        // Assert
        profile.DomainEvents.Should().ContainSingle(e => e is UserProfileUpdatedEvent);
        var updatedEvent = profile.DomainEvents.OfType<UserProfileUpdatedEvent>().Single();
        updatedEvent.ProfileId.Should().Be(profile.Id);
        updatedEvent.UpdatedField.Should().Be("DisplayName");
    }

    #endregion

    #region UpdateBirthday Tests

    [Fact]
    public void UpdateBirthday_WithValidBirthday_ShouldUpdateBirthday()
    {
        // Arrange
        var profile = CreateProfile();
        var birthday = Birthday.From(new DateOnly(1990, 6, 15));

        // Act
        profile.UpdateBirthday(birthday);

        // Assert
        profile.Birthday.Should().Be(birthday);
    }

    [Fact]
    public void UpdateBirthday_WithNull_ShouldClearBirthday()
    {
        // Arrange
        var profile = CreateProfile();
        profile.UpdateBirthday(Birthday.From(new DateOnly(1990, 6, 15)));

        // Act
        profile.UpdateBirthday(null);

        // Assert
        profile.Birthday.Should().BeNull();
    }

    [Fact]
    public void UpdateBirthday_ShouldRaiseUserProfileUpdatedEvent()
    {
        // Arrange
        var profile = CreateProfile();
        profile.ClearDomainEvents();
        var birthday = Birthday.From(new DateOnly(1990, 6, 15));

        // Act
        profile.UpdateBirthday(birthday);

        // Assert
        profile.DomainEvents.Should().ContainSingle(e => e is UserProfileUpdatedEvent);
        var updatedEvent = profile.DomainEvents.OfType<UserProfileUpdatedEvent>().Single();
        updatedEvent.UpdatedField.Should().Be("Birthday");
    }

    #endregion

    #region UpdatePronouns Tests

    [Fact]
    public void UpdatePronouns_WithValidPronouns_ShouldUpdatePronouns()
    {
        // Arrange
        var profile = CreateProfile();
        var pronouns = Pronouns.From("they/them");

        // Act
        profile.UpdatePronouns(pronouns);

        // Assert
        profile.Pronouns.Should().Be(pronouns);
    }

    [Fact]
    public void UpdatePronouns_WithNull_ShouldClearPronouns()
    {
        // Arrange
        var profile = CreateProfile();
        profile.UpdatePronouns(Pronouns.From("he/him"));

        // Act
        profile.UpdatePronouns(null);

        // Assert
        profile.Pronouns.Should().BeNull();
    }

    [Fact]
    public void UpdatePronouns_ShouldRaiseUserProfileUpdatedEvent()
    {
        // Arrange
        var profile = CreateProfile();
        profile.ClearDomainEvents();
        var pronouns = Pronouns.From("she/her");

        // Act
        profile.UpdatePronouns(pronouns);

        // Assert
        profile.DomainEvents.Should().ContainSingle(e => e is UserProfileUpdatedEvent);
        var updatedEvent = profile.DomainEvents.OfType<UserProfileUpdatedEvent>().Single();
        updatedEvent.UpdatedField.Should().Be("Pronouns");
    }

    #endregion

    #region UpdatePreferences Tests

    [Fact]
    public void UpdatePreferences_WithValidPreferences_ShouldUpdatePreferences()
    {
        // Arrange
        var profile = CreateProfile();
        var newPreferences = ProfilePreferences.Create("de", "Europe/Berlin", "dd.MM.yyyy");

        // Act
        profile.UpdatePreferences(newPreferences);

        // Assert
        profile.Preferences.Language.Should().Be("de");
        profile.Preferences.Timezone.Should().Be("Europe/Berlin");
        profile.Preferences.DateFormat.Should().Be("dd.MM.yyyy");
    }

    [Fact]
    public void UpdatePreferences_ShouldRaiseUserProfileUpdatedEvent()
    {
        // Arrange
        var profile = CreateProfile();
        profile.ClearDomainEvents();
        var newPreferences = ProfilePreferences.Create("fr", "Europe/Paris", "dd/MM/yyyy");

        // Act
        profile.UpdatePreferences(newPreferences);

        // Assert
        profile.DomainEvents.Should().ContainSingle(e => e is UserProfileUpdatedEvent);
        var updatedEvent = profile.DomainEvents.OfType<UserProfileUpdatedEvent>().Single();
        updatedEvent.UpdatedField.Should().Be("Preferences");
    }

    #endregion

    #region UpdateFieldVisibility Tests

    [Fact]
    public void UpdateFieldVisibility_WithValidVisibility_ShouldUpdateVisibility()
    {
        // Arrange
        var profile = CreateProfile();
        var newVisibility = ProfileFieldVisibility.Create(
            VisibilityLevel.Public,
            VisibilityLevel.Hidden,
            VisibilityLevel.Family);

        // Act
        profile.UpdateFieldVisibility(newVisibility);

        // Assert
        profile.FieldVisibility.BirthdayVisibility.Should().Be(VisibilityLevel.Public);
        profile.FieldVisibility.PronounsVisibility.Should().Be(VisibilityLevel.Hidden);
        profile.FieldVisibility.PreferencesVisibility.Should().Be(VisibilityLevel.Family);
    }

    [Fact]
    public void UpdateFieldVisibility_ShouldRaiseUserProfileUpdatedEvent()
    {
        // Arrange
        var profile = CreateProfile();
        profile.ClearDomainEvents();
        var newVisibility = ProfileFieldVisibility.Create(
            VisibilityLevel.Public,
            VisibilityLevel.Public,
            VisibilityLevel.Public);

        // Act
        profile.UpdateFieldVisibility(newVisibility);

        // Assert
        profile.DomainEvents.Should().ContainSingle(e => e is UserProfileUpdatedEvent);
        var updatedEvent = profile.DomainEvents.OfType<UserProfileUpdatedEvent>().Single();
        updatedEvent.UpdatedField.Should().Be("FieldVisibility");
    }

    #endregion

    #region IsFieldVisibleTo Tests

    [Fact]
    public void IsFieldVisibleTo_WhenOwnerViews_ShouldReturnTrue()
    {
        // Arrange
        var userId = UserId.New();
        var profile = UserProfileAggregate.Create(userId, DisplayName.From("John Doe"));

        // Act - Owner viewing their own profile
        var isVisible = profile.IsFieldVisibleTo("Birthday", userId, isSameFamily: false);

        // Assert
        isVisible.Should().BeTrue();
    }

    [Fact]
    public void IsFieldVisibleTo_WhenPublicField_ShouldReturnTrueForAnyone()
    {
        // Arrange
        var profile = CreateProfile();
        profile.UpdateFieldVisibility(ProfileFieldVisibility.Create(
            VisibilityLevel.Public,
            VisibilityLevel.Family,
            VisibilityLevel.Hidden));

        // Act
        var isVisible = profile.IsFieldVisibleTo("Birthday", UserId.New(), isSameFamily: false);

        // Assert
        isVisible.Should().BeTrue();
    }

    [Fact]
    public void IsFieldVisibleTo_WhenFamilyField_ShouldReturnTrueForFamilyMember()
    {
        // Arrange
        var profile = CreateProfile();
        // Default: Birthday visibility is Family

        // Act
        var isVisible = profile.IsFieldVisibleTo("Birthday", UserId.New(), isSameFamily: true);

        // Assert
        isVisible.Should().BeTrue();
    }

    [Fact]
    public void IsFieldVisibleTo_WhenFamilyField_ShouldReturnFalseForNonFamilyMember()
    {
        // Arrange
        var profile = CreateProfile();
        // Default: Birthday visibility is Family

        // Act
        var isVisible = profile.IsFieldVisibleTo("Birthday", UserId.New(), isSameFamily: false);

        // Assert
        isVisible.Should().BeFalse();
    }

    [Fact]
    public void IsFieldVisibleTo_WhenHiddenField_ShouldReturnFalseForNonOwner()
    {
        // Arrange
        var profile = CreateProfile();
        profile.UpdateFieldVisibility(ProfileFieldVisibility.Create(
            VisibilityLevel.Hidden,
            VisibilityLevel.Hidden,
            VisibilityLevel.Hidden));

        // Act - Even family members can't see hidden fields
        var isVisible = profile.IsFieldVisibleTo("Birthday", UserId.New(), isSameFamily: true);

        // Assert
        isVisible.Should().BeFalse();
    }

    [Theory]
    [InlineData("Birthday")]
    [InlineData("Pronouns")]
    [InlineData("Preferences")]
    public void IsFieldVisibleTo_WithValidFieldNames_ShouldWork(string fieldName)
    {
        // Arrange
        var userId = UserId.New();
        var profile = UserProfileAggregate.Create(userId, DisplayName.From("John Doe"));

        // Act & Assert - Should not throw
        var act = () => profile.IsFieldVisibleTo(fieldName, userId, isSameFamily: false);
        act.Should().NotThrow();
    }

    [Fact]
    public void IsFieldVisibleTo_WithUnknownFieldName_ShouldReturnFalse()
    {
        // Arrange
        var profile = CreateProfile();

        // Act
        var isVisible = profile.IsFieldVisibleTo("UnknownField", UserId.New(), isSameFamily: true);

        // Assert
        isVisible.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private static UserProfileAggregate CreateProfile()
    {
        return UserProfileAggregate.Create(
            UserId.New(),
            DisplayName.From("Test User"));
    }

    #endregion
}
