using FamilyHub.Modules.Family.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace FamilyHub.Tests.Unit.Family.Domain;

/// <summary>
/// Unit tests for the Family aggregate root.
/// Tests domain logic and business rules without dependencies.
/// </summary>
public class FamilyTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateFamily()
    {
        // Arrange
        var familyName = FamilyName.From("Smith Family");
        var ownerId = UserId.From(Guid.NewGuid());

        // Act
        var family = global::FamilyHub.Modules.Family.Domain.Family.Create(familyName, ownerId);

        // Assert
        family.Should().NotBeNull();
        family.Name.Should().Be(familyName);
        family.OwnerId.Should().Be(ownerId);
        family.Id.Should().NotBe(FamilyId.From(Guid.Empty));
        family.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void UpdateName_WithNewName_ShouldUpdateFamilyName()
    {
        // Arrange
        var family = global::FamilyHub.Modules.Family.Domain.Family.Create(
            FamilyName.From("Old Name"),
            UserId.From(Guid.NewGuid()));
        var newName = FamilyName.From("New Name");

        // Act
        family.UpdateName(newName);

        // Assert
        family.Name.Should().Be(newName);
    }

    [Fact]
    public void TransferOwnership_WithDifferentOwner_ShouldUpdateOwner()
    {
        // Arrange
        var originalOwnerId = UserId.From(Guid.NewGuid());
        var newOwnerId = UserId.From(Guid.NewGuid());
        var family = global::FamilyHub.Modules.Family.Domain.Family.Create(
            FamilyName.From("Smith Family"),
            originalOwnerId);

        // Act
        family.TransferOwnership(newOwnerId);

        // Assert
        family.OwnerId.Should().Be(newOwnerId);
    }

    [Fact]
    public void TransferOwnership_WithSameOwner_ShouldNotChangeOwner()
    {
        // Arrange
        var ownerId = UserId.From(Guid.NewGuid());
        var family = global::FamilyHub.Modules.Family.Domain.Family.Create(
            FamilyName.From("Smith Family"),
            ownerId);

        // Act
        family.TransferOwnership(ownerId);

        // Assert
        family.OwnerId.Should().Be(ownerId);
    }

    [Fact]
    public void Delete_ShouldSetDeletedAt()
    {
        // Arrange
        var family = global::FamilyHub.Modules.Family.Domain.Family.Create(
            FamilyName.From("Smith Family"),
            UserId.From(Guid.NewGuid()));
        var beforeDelete = DateTime.UtcNow;

        // Act
        family.Delete();

        // Assert
        family.DeletedAt.Should().NotBeNull();
        family.DeletedAt.Should().BeOnOrAfter(beforeDelete);
        family.DeletedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void GetMemberCount_NewFamily_ShouldReturnZero()
    {
        // Arrange
        var family = global::FamilyHub.Modules.Family.Domain.Family.Create(
            FamilyName.From("Smith Family"),
            UserId.From(Guid.NewGuid()));

        // Act
        var count = family.GetMemberCount();

        // Assert
        count.Should().Be(0);
    }
}
