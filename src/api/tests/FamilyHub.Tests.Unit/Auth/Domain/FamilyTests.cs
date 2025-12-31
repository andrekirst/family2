using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.Tests.Unit.Auth.Domain;

/// <summary>
/// Unit tests for the Family domain entity.
/// Tests domain logic, validation rules, and business invariants.
/// Uses FluentAssertions for readable, expressive test assertions.
/// </summary>
public class FamilyTests
{
    [Fact]
    public void Create_WithValidNameAndOwnerId_ShouldCreateFamily()
    {
        // Arrange
        var name = FamilyName.From("Smith Family");
        var ownerId = UserId.New();

        // Act
        var family = Family.Create(name, ownerId);

        // Assert
        family.Should().NotBeNull();
        family.Name.Should().Be(name);
        family.OwnerId.Should().Be(ownerId);
        family.Id.Value.Should().NotBe(Guid.Empty);

        var now = DateTime.UtcNow;
        family.CreatedAt.Should().BeOnOrBefore(now);
        family.UpdatedAt.Should().BeOnOrBefore(now);

        family.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithNameContainingWhitespace_ShouldTrimWhitespace()
    {
        // Arrange
        var nameWithWhitespace = "  Smith Family  ";
        var expectedName = FamilyName.From("Smith Family");
        var ownerId = UserId.New();

        // Act
        var family = Family.Create(FamilyName.From(nameWithWhitespace), ownerId);

        // Assert
        family.Name.Should().Be(expectedName);
    }

    [Fact]
    public void Create_WithNullName_ShouldThrowValueObjectValidationException()
    {
        // Arrange
        var ownerId = UserId.New();
        string? nullName = null;

        // Act - Vogen throws exception when creating the value object
        var act = () => FamilyName.From(nullName!);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*Cannot create a value object with null*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_WithEmptyOrWhitespaceName_ShouldThrowValueObjectValidationException(string invalidName)
    {
        // Arrange - Act
        var act = () => FamilyName.From(invalidName);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*Family name cannot be empty*");
    }

    [Fact]
    public void Create_WithNameExceeding100Characters_ShouldThrowValueObjectValidationException()
    {
        // Arrange
        var longName = new string('A', 101);

        // Act
        var act = () => FamilyName.From(longName);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*Family name cannot exceed 100 characters*");
    }

    [Fact]
    public void Create_WithNameExactly100Characters_ShouldSucceed()
    {
        // Arrange
        var maxLengthName = new string('A', 100);
        var ownerId = UserId.New();

        // Act
        var family = Family.Create(FamilyName.From(maxLengthName), ownerId);

        // Assert
        family.Should().NotBeNull();
        family.Name.Value.Should().Be(maxLengthName);
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateNameAndTimestamp()
    {
        // Arrange
        var family = Family.Create(FamilyName.From("Original Name"), UserId.New());
        var originalUpdatedAt = family.UpdatedAt;
        var newName = FamilyName.From("Updated Name");

        // Small delay to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        family.UpdateName(newName);

        // Assert
        family.Name.Should().Be(newName);
        family.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateName_WithNameContainingWhitespace_ShouldTrimWhitespace()
    {
        // Arrange
        var family = Family.Create(FamilyName.From("Original Name"), UserId.New());
        var nameWithWhitespace = "  Updated Name  ";
        var expectedName = FamilyName.From("Updated Name");

        // Act
        family.UpdateName(FamilyName.From(nameWithWhitespace));

        // Assert
        family.Name.Should().Be(expectedName);
    }

    [Fact]
    public void UpdateName_WithNullName_ShouldThrowValueObjectValidationException()
    {
        // Arrange
        var family = Family.Create(FamilyName.From("Original Name"), UserId.New());
        string? nullName = null;

        // Act - Vogen throws exception when creating the value object
        var act = () => FamilyName.From(nullName!);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*Cannot create a value object with null*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_WithEmptyOrWhitespaceName_ShouldThrowValueObjectValidationException(string invalidName)
    {
        // Arrange - Act
        var act = () => FamilyName.From(invalidName);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*Family name cannot be empty*");
    }

    [Fact]
    public void UpdateName_WithNameExceeding100Characters_ShouldThrowValueObjectValidationException()
    {
        // Arrange
        var longName = new string('B', 101);

        // Act
        var act = () => FamilyName.From(longName);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*Family name cannot exceed 100 characters*");
    }

    [Fact]
    public void TransferOwnership_WithDifferentOwnerId_ShouldUpdateOwnerAndTimestamp()
    {
        // Arrange
        var originalOwnerId = UserId.New();
        var family = Family.Create(FamilyName.From("Smith Family"), originalOwnerId);
        var newOwnerId = UserId.New();
        var originalUpdatedAt = family.UpdatedAt;

        // Small delay to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        family.TransferOwnership(newOwnerId);

        // Assert
        family.OwnerId.Should().Be(newOwnerId);
        family.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void TransferOwnership_WithSameOwnerId_ShouldNotUpdateAnything()
    {
        // Arrange
        var ownerId = UserId.New();
        var family = Family.Create(FamilyName.From("Smith Family"), ownerId);
        var originalUpdatedAt = family.UpdatedAt;

        // Small delay
        Thread.Sleep(10);

        // Act
        family.TransferOwnership(ownerId);

        // Assert
        family.OwnerId.Should().Be(ownerId);
        family.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void Delete_ShouldSetDeletedAtAndUpdateTimestamp()
    {
        // Arrange
        var family = Family.Create(FamilyName.From("Smith Family"), UserId.New());
        var originalUpdatedAt = family.UpdatedAt;

        // Small delay to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        family.Delete();

        // Assert
        family.DeletedAt.Should().NotBeNull()
            .And.BeOnOrBefore(DateTime.UtcNow);
        family.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UserFamily_CreateOwnerMembership_ShouldCreateOwnerWithCorrectRole()
    {
        // Arrange
        var ownerId = UserId.New();
        var familyId = FamilyId.New();

        // Act
        var ownerMembership = UserFamily.CreateOwnerMembership(ownerId, familyId);

        // Assert
        ownerMembership.Should().NotBeNull();
        ownerMembership.UserId.Should().Be(ownerId);
        ownerMembership.FamilyId.Should().Be(familyId);
        ownerMembership.Role.Should().Be(UserRole.Owner);
        ownerMembership.IsActive.Should().BeTrue();
        ownerMembership.InvitedBy.Should().BeNull();
        ownerMembership.JoinedAt.Should().BeOnOrBefore(DateTime.UtcNow);
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
        membership.Should().NotBeNull();
        membership.UserId.Should().Be(memberId);
        membership.FamilyId.Should().Be(familyId);
        membership.Role.Should().Be(UserRole.Member);
        membership.IsActive.Should().BeTrue();
        membership.InvitedBy.Should().Be(invitedBy);
        membership.JoinedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void UserFamily_CreateMembership_WithOwnerRole_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var memberId = UserId.New();
        var familyId = FamilyId.New();
        var invitedBy = UserId.New();

        // Act
        var act = () => UserFamily.CreateMembership(memberId, familyId, UserRole.Owner, invitedBy);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Use CreateOwnerMembership for owner role*");
    }

    [Fact]
    public void UserFamilies_ShouldBeReadOnly()
    {
        // Arrange
        var family = Family.Create(FamilyName.From("Smith Family"), UserId.New());

        // Assert
        family.UserFamilies.Should().BeAssignableTo<IReadOnlyCollection<UserFamily>>();
    }

    [Fact]
    public void UserFamilies_InitiallyEmpty()
    {
        // Arrange & Act
        var family = Family.Create(FamilyName.From("Smith Family"), UserId.New());

        // Assert
        family.UserFamilies.Should().BeEmpty();
    }
}
