using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using FamilyAggregate = FamilyHub.Modules.Family.Domain.Aggregates.Family;

namespace FamilyHub.Tests.Unit.Auth.Domain;

/// <summary>
/// Unit tests for the FamilyAggregate domain entity.
/// Tests domain logic, validation rules, and business invariants.
/// Uses FluentAssertions for readable, expressive test assertions.
/// </summary>
public class FamilyTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidNameAndOwnerId_ShouldCreateFamily()
    {
        // Arrange
        var name = FamilyName.From("Smith Family");
        var ownerId = UserId.New();

        // Act
        var family = FamilyAggregate.Create(name, ownerId);

        // Assert
        family.Should().NotBeNull();
        family.Name.Should().Be(name);
        family.OwnerId.Should().Be(ownerId);
        family.Id.Value.Should().NotBe(Guid.Empty);
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
        var family = FamilyAggregate.Create(FamilyName.From(nameWithWhitespace), ownerId);

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
        var family = FamilyAggregate.Create(FamilyName.From(maxLengthName), ownerId);

        // Assert
        family.Should().NotBeNull();
        family.Name.Value.Should().Be(maxLengthName);
    }

    #endregion

    #region UpdateName Tests

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var family = FamilyAggregate.Create(FamilyName.From("Original Name"), UserId.New());
        var newName = FamilyName.From("Updated Name");

        // Act
        family.UpdateName(newName);

        // Assert
        family.Name.Should().Be(newName);
    }

    [Fact]
    public void UpdateName_WithNameContainingWhitespace_ShouldTrimWhitespace()
    {
        // Arrange
        var family = FamilyAggregate.Create(FamilyName.From("Original Name"), UserId.New());
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
        var family = FamilyAggregate.Create(FamilyName.From("Original Name"), UserId.New());
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

    #endregion

    #region TransferOwnership Tests

    [Fact]
    public void TransferOwnership_WithDifferentOwnerId_ShouldUpdateOwner()
    {
        // Arrange
        var originalOwnerId = UserId.New();
        var family = FamilyAggregate.Create(FamilyName.From("Smith Family"), originalOwnerId);
        var newOwnerId = UserId.New();

        // Act
        family.TransferOwnership(newOwnerId);

        // Assert
        family.OwnerId.Should().Be(newOwnerId);
    }

    [Fact]
    public void TransferOwnership_WithSameOwnerId_ShouldNotUpdateOwner()
    {
        // Arrange
        var ownerId = UserId.New();
        var family = FamilyAggregate.Create(FamilyName.From("Smith Family"), ownerId);

        // Act
        family.TransferOwnership(ownerId);

        // Assert
        family.OwnerId.Should().Be(ownerId);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Delete_ShouldSetDeletedAt()
    {
        // Arrange
        var family = FamilyAggregate.Create(FamilyName.From("Smith Family"), UserId.New());

        // Act
        family.Delete();

        // Assert
        family.DeletedAt.Should().NotBeNull()
            .And.BeOnOrBefore(DateTime.UtcNow);
    }

    #endregion

    // NOTE: Members Collection Tests removed
    // The Family aggregate no longer has a Members collection or GetMemberCount method.
    // Member relationships are now managed by the Auth module through User.FamilyId foreign key.
    // This maintains proper bounded context separation between the Family module (aggregate ownership)
    // and the Auth module (user management).
}
