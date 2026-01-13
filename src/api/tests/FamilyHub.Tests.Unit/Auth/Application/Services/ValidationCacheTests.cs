using FamilyHub.Modules.Auth.Application.Services;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;
using FamilyMemberInvitationAggregate = FamilyHub.Modules.Family.Domain.Aggregates.FamilyMemberInvitation;

namespace FamilyHub.Tests.Unit.Auth.Application.Services;

/// <summary>
/// Unit tests for ValidationCache.
/// Tests caching functionality, type safety, and Clear operation.
/// </summary>
public sealed class ValidationCacheTests
{
    #region Set/Get Tests

    [Fact]
    public void Set_AndGet_WithValidEntity_ShouldReturnSameInstance()
    {
        // Arrange
        var cache = new ValidationCache();
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var family = FamilyAggregate.Create(FamilyName.From("Test Family"), userId);
        var key = $"Family:{familyId.Value}";

        // Act
        cache.Set(key, family);
        var result = cache.Get<FamilyAggregate>(key);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(family); // Same reference (not a copy)
    }

    [Fact]
    public void Get_WithNonExistentKey_ShouldReturnNull()
    {
        // Arrange
        var cache = new ValidationCache();
        var key = "NonExistent:123";

        // Act
        var result = cache.Get<FamilyAggregate>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Get_WithWrongType_ShouldReturnNull()
    {
        // Arrange
        var cache = new ValidationCache();
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var family = FamilyAggregate.Create(FamilyName.From("Test Family"), userId);
        var key = $"Family:{familyId.Value}";

        // Act
        cache.Set(key, family);
        var result = cache.Get<User>(key); // Wrong type - stored Family, asking for User

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Set_WithNullKey_ShouldThrowArgumentNullException()
    {
        // Arrange
        var cache = new ValidationCache();
        FamilyId.New();
        var userId = UserId.New();
        var family = FamilyAggregate.Create(FamilyName.From("Test Family"), userId);

        // Act
        var act = () => cache.Set(null!, family);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("key");
    }

    [Fact]
    public void Set_WithNullEntity_ShouldThrowArgumentNullException()
    {
        // Arrange
        var cache = new ValidationCache();
        var key = "Family:123";

        // Act
        var act = () => cache.Set(key, (FamilyAggregate)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("entity");
    }

    [Fact]
    public void Get_WithNullKey_ShouldThrowArgumentNullException()
    {
        // Arrange
        var cache = new ValidationCache();

        // Act
        var act = () => cache.Get<FamilyAggregate>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("key");
    }

    #endregion

    #region TryGet Tests

    [Fact]
    public void TryGet_WithExistingEntity_ShouldReturnTrueAndEntity()
    {
        // Arrange
        var cache = new ValidationCache();
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var family = FamilyAggregate.Create(FamilyName.From("Test Family"), userId);
        var key = $"Family:{familyId.Value}";

        cache.Set(key, family);

        // Act
        var success = cache.TryGet<FamilyAggregate>(key, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result.Should().BeSameAs(family);
    }

    [Fact]
    public void TryGet_WithNonExistentKey_ShouldReturnFalseAndNullEntity()
    {
        // Arrange
        var cache = new ValidationCache();
        var key = "NonExistent:123";

        // Act
        var success = cache.TryGet<FamilyAggregate>(key, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void TryGet_WithWrongType_ShouldReturnFalseAndNullEntity()
    {
        // Arrange
        var cache = new ValidationCache();
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var family = FamilyAggregate.Create(FamilyName.From("Test Family"), userId);
        var key = $"Family:{familyId.Value}";

        cache.Set(key, family);

        // Act
        var success = cache.TryGet<User>(key, out var result); // Wrong type

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void TryGet_WithNullKey_ShouldThrowArgumentNullException()
    {
        // Arrange
        var cache = new ValidationCache();

        // Act
        var act = () => cache.TryGet<FamilyAggregate>(null!, out _);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("key");
    }

    #endregion

    #region Clear Tests

    [Fact]
    public void Clear_WithMultipleEntities_ShouldRemoveAllEntities()
    {
        // Arrange
        var cache = new ValidationCache();
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var family = FamilyAggregate.Create(FamilyName.From("Test Family"), userId);
        var user = User.CreateFromOAuth(Email.From("test@example.com"), "ext-123", "zitadel", familyId);

        cache.Set($"Family:{familyId.Value}", family);
        cache.Set($"User:{userId.Value}", user);

        // Act
        cache.Clear();

        // Assert
        cache.Get<FamilyAggregate>($"Family:{familyId.Value}").Should().BeNull();
        cache.Get<User>($"User:{userId.Value}").Should().BeNull();
    }

    [Fact]
    public void Clear_WithEmptyCache_ShouldNotThrow()
    {
        // Arrange
        var cache = new ValidationCache();

        // Act
        var act = () => cache.Clear();

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region Multiple Entity Tests

    [Fact]
    public void Set_WithMultipleDifferentEntities_ShouldStoreAllIndependently()
    {
        // Arrange
        var cache = new ValidationCache();
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var invitedByUserId = UserId.New();
        var email = Email.From("test@example.com");

        var family = FamilyAggregate.Create(FamilyName.From("Test Family"), userId);
        var user = User.CreateFromOAuth(email, "ext-123", "zitadel", familyId);
        var invitation = FamilyMemberInvitationAggregate.CreateEmailInvitation(
            familyId, email, FamilyRole.Member, invitedByUserId);

        // Act
        cache.Set($"Family:{familyId.Value}", family);
        cache.Set($"User:{userId.Value}", user);
        cache.Set($"FamilyMemberInvitation:{invitation.Token.Value}", invitation);

        // Assert
        cache.Get<FamilyAggregate>($"Family:{familyId.Value}").Should().BeSameAs(family);
        cache.Get<User>($"User:{userId.Value}").Should().BeSameAs(user);
        cache.Get<FamilyMemberInvitationAggregate>($"FamilyMemberInvitation:{invitation.Token.Value}")
            .Should().BeSameAs(invitation);
    }

    [Fact]
    public void Set_WithSameKeyTwice_ShouldOverwritePreviousValue()
    {
        // Arrange
        var cache = new ValidationCache();
        var familyId = FamilyId.New();
        var userId1 = UserId.New();
        var userId2 = UserId.New();

        var family1 = FamilyAggregate.Create(FamilyName.From("First Family"), userId1);
        var family2 = FamilyAggregate.Create(FamilyName.From("Second Family"), userId2);
        var key = $"Family:{familyId.Value}";

        // Act
        cache.Set(key, family1);
        cache.Set(key, family2); // Overwrite

        var result = cache.Get<FamilyAggregate>(key);

        // Assert
        result.Should().BeSameAs(family2);
        result.Should().NotBeSameAs(family1);
    }

    #endregion

    #region Type Safety Tests

    [Fact]
    public void Cache_WithDifferentTypesForSameKey_ShouldBeTypeSafe()
    {
        // Arrange
        var cache = new ValidationCache();
        FamilyId.New();
        var userId = UserId.New();
        var family = FamilyAggregate.Create(FamilyName.From("Test Family"), userId);
        var key = "SharedKey:123";

        // Act
        cache.Set(key, family);

        // Assert
        cache.Get<FamilyAggregate>(key).Should().BeSameAs(family); // Correct type
        cache.Get<User>(key).Should().BeNull(); // Wrong type returns null
        cache.TryGet<User>(key, out var userResult).Should().BeFalse();
        userResult.Should().BeNull();
    }

    #endregion
}
