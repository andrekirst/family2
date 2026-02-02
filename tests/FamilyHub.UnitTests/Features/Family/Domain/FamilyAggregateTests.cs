using FamilyHub.Api.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Events;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace FamilyHub.UnitTests.Features.Family.Domain;

/// <summary>
/// Unit tests for Family aggregate root.
/// Tests factory method and domain events.
/// </summary>
public class FamilyAggregateTests
{
    [Fact]
    public void Create_ShouldCreateFamilyWithValidData()
    {
        // Arrange
        var name = FamilyName.From("Test Family");
        var ownerId = UserId.New();

        // Act
        var family = FamilyHub.Api.Features.Family.Domain.Entities.Family.Create(name, ownerId);

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
        var name = FamilyName.From("Test Family");
        var ownerId = UserId.New();

        // Act
        var family = FamilyHub.Api.Features.Family.Domain.Entities.Family.Create(name, ownerId);

        // Assert
        family.DomainEvents.Should().HaveCount(1);
        var domainEvent = family.DomainEvents.First();
        domainEvent.Should().BeOfType<FamilyCreatedEvent>();

        var familyCreatedEvent = (FamilyCreatedEvent)domainEvent;
        familyCreatedEvent.FamilyId.Should().Be(family.Id);
        familyCreatedEvent.Name.Should().Be(name);
        familyCreatedEvent.OwnerId.Should().Be(ownerId);
    }

    // Future tests (add when implementing corresponding features):
    // - AddMember tests (when AddMember method is implemented)
    // - RemoveMember tests (when RemoveMember method is implemented)
    // - Rename tests (when Rename method is implemented)
    // - TransferOwnership tests (when TransferOwnership method is implemented)
}
