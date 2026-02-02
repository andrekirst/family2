using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.Events;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Events;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace FamilyHub.UnitTests.Features.Family.Domain;

/// <summary>
/// Unit tests for Family aggregate root.
/// Tests factory methods, domain logic, member management, and domain events.
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

    [Fact]
    public void AddMember_ShouldAddUserAndRaiseEvent()
    {
        // Arrange
        var family = CreateTestFamily();
        var user = CreateTestUser();

        // Act
        family.AddMember(user);

        // Assert
        family.Members.Should().Contain(user);
        // Note: user.FamilyId is set via user.AssignToFamily() called within AddMember()
        user.FamilyId.Should().NotBeNull();
        user.FamilyId!.Value.Should().Be(family.Id);
        family.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

        // Verify events raised (1 event on family: FamilyMemberAdded - FamilyCreated was cleared)
        family.DomainEvents.Should().HaveCount(1);
        var memberAddedEvent = family.DomainEvents.OfType<FamilyMemberAddedEvent>().FirstOrDefault();
        memberAddedEvent.Should().NotBeNull();
        memberAddedEvent!.FamilyId.Should().Be(family.Id);
        memberAddedEvent.UserId.Should().Be(user.Id);

        // User also has an event (UserFamilyAssigned)
        user.DomainEvents.Should().HaveCount(1);
        user.DomainEvents.First().Should().BeOfType<UserFamilyAssignedEvent>();
    }

    [Fact]
    public void AddMember_WhenAlreadyMember_ShouldThrowDomainException()
    {
        // Arrange
        var family = CreateTestFamily();
        var user = CreateTestUser();
        family.AddMember(user);

        // Act & Assert
        var act = () => family.AddMember(user);
        act.Should().Throw<DomainException>()
            .WithMessage($"User {user.Id.Value} is already a member of this family");
    }

    [Fact]
    public void RemoveMember_ShouldRemoveUserAndRaiseEvent()
    {
        // Arrange
        var family = CreateTestFamily();
        var user = CreateTestUser();
        family.AddMember(user);
        family.ClearDomainEvents(); // Clear previous events

        // Act
        family.RemoveMember(user);

        // Assert
        family.Members.Should().NotContain(user);
        family.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

        family.DomainEvents.Should().HaveCount(1);
        var memberRemovedEvent = family.DomainEvents.OfType<FamilyMemberRemovedEvent>().FirstOrDefault();
        memberRemovedEvent.Should().NotBeNull();
        memberRemovedEvent!.FamilyId.Should().Be(family.Id);
        memberRemovedEvent.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void RemoveMember_WhenNotMember_ShouldThrowDomainException()
    {
        // Arrange
        var family = CreateTestFamily();
        var user = CreateTestUser();

        // Act & Assert
        var act = () => family.RemoveMember(user);
        act.Should().Throw<DomainException>()
            .WithMessage($"User {user.Id.Value} is not a member of this family");
    }

    [Fact]
    public void RemoveMember_WhenOwner_ShouldThrowDomainException()
    {
        // Arrange
        var ownerId = UserId.New();
        var family = FamilyHub.Api.Features.Family.Domain.Entities.Family.Create(
            FamilyName.From("Test Family"), ownerId);

        var owner = User.Register(
            Email.From("owner@example.com"),
            UserName.From("Owner"),
            ExternalUserId.From("owner-123"),
            emailVerified: true);

        // Simulate owner in members collection (EF Core would do this via navigation)
        var membersField = typeof(FamilyHub.Api.Features.Family.Domain.Entities.Family)
            .GetProperty("Members")!
            .GetValue(family) as ICollection<User>;

        // Set owner ID to match family owner
        var ownerIdField = typeof(User).GetProperty("Id")!;
        ownerIdField.SetValue(owner, ownerId);

        membersField!.Add(owner);

        // Act & Assert
        var act = () => family.RemoveMember(owner);
        act.Should().Throw<DomainException>()
            .WithMessage("Cannot remove the family owner");
    }

    [Fact]
    public void Rename_ShouldUpdateFamilyName()
    {
        // Arrange
        var family = CreateTestFamily();
        var newName = FamilyName.From("Updated Family Name");

        // Act
        family.Rename(newName);

        // Assert
        family.Name.Should().Be(newName);
        family.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void TransferOwnership_WhenNewOwnerIsMember_ShouldUpdateOwner()
    {
        // Arrange
        var family = CreateTestFamily();
        var newOwner = CreateTestUser();
        family.AddMember(newOwner);

        // Act
        family.TransferOwnership(newOwner.Id);

        // Assert
        family.OwnerId.Should().Be(newOwner.Id);
        family.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void TransferOwnership_WhenNewOwnerNotMember_ShouldThrowDomainException()
    {
        // Arrange
        var family = CreateTestFamily();
        var nonMemberId = UserId.New();

        // Act & Assert
        var act = () => family.TransferOwnership(nonMemberId);
        act.Should().Throw<DomainException>()
            .WithMessage("New owner must be a family member");
    }

    private static FamilyHub.Api.Features.Family.Domain.Entities.Family CreateTestFamily()
    {
        var name = FamilyName.From("Test Family");
        var ownerId = UserId.New();
        var family = FamilyHub.Api.Features.Family.Domain.Entities.Family.Create(name, ownerId);
        family.ClearDomainEvents(); // Clear creation event for cleaner tests
        return family;
    }

    private static User CreateTestUser()
    {
        var email = Email.From($"user-{Guid.NewGuid()}@example.com");
        var name = UserName.From("Test User");
        var externalId = ExternalUserId.From($"external-{Guid.NewGuid()}");
        var user = User.Register(email, name, externalId, emailVerified: true);
        user.ClearDomainEvents(); // Clear registration event for cleaner tests
        return user;
    }
}
