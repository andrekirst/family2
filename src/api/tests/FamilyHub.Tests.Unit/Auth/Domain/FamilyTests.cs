using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Tests.Unit.Auth.Domain;

/// <summary>
/// Unit tests for the Family domain entity.
/// Tests domain logic, validation rules, and business invariants.
/// </summary>
public class FamilyTests
{
    private readonly IFixture _fixture;

    public FamilyTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    #region Create Tests

    [Fact]
    public void Create_WithValidNameAndOwnerId_ShouldCreateFamily()
    {
        // Arrange
        var name = "Smith Family";
        var ownerId = UserId.New();

        // Act
        var family = Family.Create(name, ownerId);

        // Assert
        Assert.NotNull(family);
        Assert.Equal(name, family.Name);
        Assert.Equal(ownerId, family.OwnerId);
        Assert.NotEqual(Guid.Empty, family.Id.Value);
        var now = DateTime.UtcNow;
        Assert.True(family.CreatedAt <= now);
        Assert.True(family.UpdatedAt <= now);
        // CreatedAt and UpdatedAt should be equal or very close (within 100ms)
        var timeDiff = (family.UpdatedAt - family.CreatedAt).TotalMilliseconds;
        Assert.True(Math.Abs(timeDiff) < 100);
        Assert.Null(family.DeletedAt);
    }

    [Fact]
    public void Create_WithNameContainingWhitespace_ShouldTrimWhitespace()
    {
        // Arrange
        var nameWithWhitespace = "  Smith Family  ";
        var expectedName = "Smith Family";
        var ownerId = UserId.New();

        // Act
        var family = Family.Create(nameWithWhitespace, ownerId);

        // Assert
        Assert.Equal(expectedName, family.Name);
    }

    [Fact]
    public void Create_WithNullName_ShouldThrowArgumentException()
    {
        // Arrange
        var ownerId = UserId.New();
        string? nullName = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Family.Create(nullName!, ownerId));
        Assert.Equal("name", exception.ParamName);
        Assert.Contains("Family name cannot be empty", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_WithEmptyOrWhitespaceName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var ownerId = UserId.New();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Family.Create(invalidName, ownerId));
        Assert.Equal("name", exception.ParamName);
        Assert.Contains("Family name cannot be empty", exception.Message);
    }

    [Fact]
    public void Create_WithNameExceeding100Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var longName = new string('A', 101);
        var ownerId = UserId.New();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Family.Create(longName, ownerId));
        Assert.Equal("name", exception.ParamName);
        Assert.Contains("Family name cannot exceed 100 characters", exception.Message);
    }

    [Fact]
    public void Create_WithNameExactly100Characters_ShouldSucceed()
    {
        // Arrange
        var maxLengthName = new string('A', 100);
        var ownerId = UserId.New();

        // Act
        var family = Family.Create(maxLengthName, ownerId);

        // Assert
        Assert.NotNull(family);
        Assert.Equal(maxLengthName, family.Name);
    }

    #endregion

    #region UpdateName Tests

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateNameAndTimestamp()
    {
        // Arrange
        var family = Family.Create("Original Name", UserId.New());
        var originalUpdatedAt = family.UpdatedAt;
        var newName = "Updated Name";

        // Small delay to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        family.UpdateName(newName);

        // Assert
        Assert.Equal(newName, family.Name);
        Assert.True(family.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void UpdateName_WithNameContainingWhitespace_ShouldTrimWhitespace()
    {
        // Arrange
        var family = Family.Create("Original Name", UserId.New());
        var nameWithWhitespace = "  Updated Name  ";
        var expectedName = "Updated Name";

        // Act
        family.UpdateName(nameWithWhitespace);

        // Assert
        Assert.Equal(expectedName, family.Name);
    }

    [Fact]
    public void UpdateName_WithNullName_ShouldThrowArgumentException()
    {
        // Arrange
        var family = Family.Create("Original Name", UserId.New());
        string? nullName = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => family.UpdateName(nullName!));
        Assert.Equal("newName", exception.ParamName);
        Assert.Contains("Family name cannot be empty", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_WithEmptyOrWhitespaceName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var family = Family.Create("Original Name", UserId.New());

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => family.UpdateName(invalidName));
        Assert.Equal("newName", exception.ParamName);
        Assert.Contains("Family name cannot be empty", exception.Message);
    }

    [Fact]
    public void UpdateName_WithNameExceeding100Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var family = Family.Create("Original Name", UserId.New());
        var longName = new string('B', 101);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => family.UpdateName(longName));
        Assert.Equal("newName", exception.ParamName);
        Assert.Contains("Family name cannot exceed 100 characters", exception.Message);
    }

    #endregion

    #region TransferOwnership Tests

    [Fact]
    public void TransferOwnership_WithDifferentOwnerId_ShouldUpdateOwnerAndTimestamp()
    {
        // Arrange
        var originalOwnerId = UserId.New();
        var family = Family.Create("Smith Family", originalOwnerId);
        var newOwnerId = UserId.New();
        var originalUpdatedAt = family.UpdatedAt;

        // Small delay to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        family.TransferOwnership(newOwnerId);

        // Assert
        Assert.Equal(newOwnerId, family.OwnerId);
        Assert.True(family.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void TransferOwnership_WithSameOwnerId_ShouldNotUpdateAnything()
    {
        // Arrange
        var ownerId = UserId.New();
        var family = Family.Create("Smith Family", ownerId);
        var originalUpdatedAt = family.UpdatedAt;

        // Small delay
        Thread.Sleep(10);

        // Act
        family.TransferOwnership(ownerId);

        // Assert
        Assert.Equal(ownerId, family.OwnerId);
        Assert.Equal(originalUpdatedAt, family.UpdatedAt);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Delete_ShouldSetDeletedAtAndUpdateTimestamp()
    {
        // Arrange
        var family = Family.Create("Smith Family", UserId.New());
        var originalUpdatedAt = family.UpdatedAt;

        // Small delay to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        family.Delete();

        // Assert
        Assert.NotNull(family.DeletedAt);
        Assert.True(family.DeletedAt <= DateTime.UtcNow);
        Assert.True(family.UpdatedAt > originalUpdatedAt);
    }

    #endregion

    #region UserFamily Factory Method Tests

    [Fact]
    public void UserFamily_CreateOwnerMembership_ShouldCreateOwnerWithCorrectRole()
    {
        // Arrange
        var ownerId = UserId.New();
        var familyId = FamilyId.New();

        // Act
        var ownerMembership = UserFamily.CreateOwnerMembership(ownerId, familyId);

        // Assert
        Assert.NotNull(ownerMembership);
        Assert.Equal(ownerId, ownerMembership.UserId);
        Assert.Equal(familyId, ownerMembership.FamilyId);
        Assert.Equal(UserRole.Owner, ownerMembership.Role);
        Assert.True(ownerMembership.IsActive);
        Assert.Null(ownerMembership.InvitedBy);
        Assert.True(ownerMembership.JoinedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void UserFamily_CreateMembership_ShouldCreateMemberWithCorrectRole()
    {
        // Arrange
        var memberId = UserId.New();
        var familyId = FamilyId.New();
        var invitedBy = UserId.New();

        // Act
        var membership = UserFamily.CreateMembership(
            memberId,
            familyId,
            UserRole.Member,
            invitedBy);

        // Assert
        Assert.NotNull(membership);
        Assert.Equal(memberId, membership.UserId);
        Assert.Equal(familyId, membership.FamilyId);
        Assert.Equal(UserRole.Member, membership.Role);
        Assert.True(membership.IsActive);
        Assert.Equal(invitedBy, membership.InvitedBy);
        Assert.True(membership.JoinedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void UserFamily_CreateMembership_WithOwnerRole_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var memberId = UserId.New();
        var familyId = FamilyId.New();
        var invitedBy = UserId.New();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            UserFamily.CreateMembership(memberId, familyId, UserRole.Owner, invitedBy));

        Assert.Contains("Use CreateOwnerMembership for owner role", exception.Message);
    }

    #endregion

    #region UserFamilies Collection Tests

    [Fact]
    public void UserFamilies_ShouldBeReadOnly()
    {
        // Arrange
        var family = Family.Create("Smith Family", UserId.New());

        // Act & Assert
        Assert.IsType<IReadOnlyCollection<UserFamily>>(family.UserFamilies, exactMatch: false);
    }

    [Fact]
    public void UserFamilies_InitiallyEmpty()
    {
        // Arrange & Act
        var family = Family.Create("Smith Family", UserId.New());

        // Assert
        Assert.Empty(family.UserFamilies);
    }

    #endregion
}
