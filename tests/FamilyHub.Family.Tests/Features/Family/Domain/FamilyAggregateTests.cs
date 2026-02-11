using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Events;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FluentAssertions;
using FamilyEntity = FamilyHub.Api.Features.Family.Domain.Entities.Family;

namespace FamilyHub.Family.Tests.Features.Family.Domain;

public class FamilyAggregateTests
{
    [Fact]
    public void Create_ShouldCreateFamilyWithValidData()
    {
        // Arrange
        var name = FamilyName.From("Smith Family");
        var ownerId = UserId.New();

        // Act
        var family = FamilyEntity.Create(name, ownerId);

        // Assert
        family.Should().NotBeNull();
        family.Id.Value.Should().NotBe(Guid.Empty);
        family.Name.Should().Be(name);
        family.OwnerId.Should().Be(ownerId);
        family.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        family.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ShouldRaiseFamilyCreatedEvent()
    {
        // Arrange
        var name = FamilyName.From("Smith Family");
        var ownerId = UserId.New();

        // Act
        var family = FamilyEntity.Create(name, ownerId);

        // Assert
        family.DomainEvents.Should().HaveCount(1);
        var domainEvent = family.DomainEvents.First();
        domainEvent.Should().BeOfType<FamilyCreatedEvent>();

        var familyCreatedEvent = (FamilyCreatedEvent)domainEvent;
        familyCreatedEvent.FamilyId.Should().Be(family.Id);
        familyCreatedEvent.FamilyName.Should().Be(name);
        familyCreatedEvent.OwnerId.Should().Be(ownerId);
        familyCreatedEvent.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange
        var name = FamilyName.From("Smith Family");
        var ownerId = UserId.New();

        // Act
        var family1 = FamilyEntity.Create(name, ownerId);
        var family2 = FamilyEntity.Create(name, ownerId);

        // Assert
        family1.Id.Should().NotBe(family2.Id);
    }
}
