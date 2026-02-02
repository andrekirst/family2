using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.Events;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Domain.Entities;

/// <summary>
/// Family aggregate root representing a household unit in the Family Hub.
/// Encapsulates family membership management and enforces family invariants.
/// </summary>
public sealed class Family : AggregateRoot<FamilyId>
{
    // Private parameterless constructor for EF Core
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    private Family() { }
#pragma warning restore CS8618

    /// <summary>
    /// Family name (e.g., "Smith Family")
    /// </summary>
    public FamilyName Name { get; private set; }

    /// <summary>
    /// User ID of the family owner (creator)
    /// </summary>
    public UserId OwnerId { get; private set; }

    /// <summary>
    /// When the family was created
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// When the family record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Navigation property to the family owner
    /// </summary>
    public Auth.Domain.Entities.User Owner { get; private set; } = null!;

    /// <summary>
    /// Navigation property to all family members
    /// </summary>
    public ICollection<Auth.Domain.Entities.User> Members { get; private set; } = new List<Auth.Domain.Entities.User>();

    /// <summary>
    /// Factory method to create a new family with an owner.
    /// Raises FamilyCreatedEvent.
    /// </summary>
    public static Family Create(FamilyName name, UserId ownerId)
    {
        var family = new Family
        {
            Id = FamilyId.New(),
            Name = name,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        family.RaiseDomainEvent(new FamilyCreatedEvent(
            family.Id,
            family.Name,
            family.OwnerId,
            DateTime.UtcNow
        ));

        return family;
    }

    /// <summary>
    /// Add a member to the family.
    /// Calls user.AssignToFamily() to maintain consistency.
    /// Raises FamilyMemberAddedEvent and UserFamilyAssignedEvent.
    /// </summary>
    /// <exception cref="DomainException">If user is already a member</exception>
    public void AddMember(Auth.Domain.Entities.User user)
    {
        if (Members.Any(m => m.Id == user.Id))
        {
            throw new DomainException($"User {user.Id.Value} is already a member of this family");
        }

        // Update the user's family assignment (raises UserFamilyAssignedEvent)
        user.AssignToFamily(Id);

        // Add to members collection
        Members.Add(user);
        UpdatedAt = DateTime.UtcNow;

        // Raise family-side event
        RaiseDomainEvent(new FamilyMemberAddedEvent(
            Id,
            user.Id,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Remove a member from the family.
    /// Raises FamilyMemberRemovedEvent.
    /// </summary>
    /// <exception cref="DomainException">If user is not a member or is the owner</exception>
    public void RemoveMember(Auth.Domain.Entities.User user)
    {
        if (user.Id == OwnerId)
        {
            throw new DomainException("Cannot remove the family owner");
        }

        var member = Members.FirstOrDefault(m => m.Id == user.Id);
        if (member is null)
        {
            throw new DomainException($"User {user.Id.Value} is not a member of this family");
        }

        Members.Remove(member);
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new FamilyMemberRemovedEvent(
            Id,
            user.Id,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Rename the family.
    /// </summary>
    public void Rename(FamilyName newName)
    {
        Name = newName;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Transfer ownership to another member.
    /// </summary>
    /// <exception cref="DomainException">If new owner is not a member</exception>
    public void TransferOwnership(UserId newOwnerId)
    {
        if (!Members.Any(m => m.Id == newOwnerId))
        {
            throw new DomainException("New owner must be a family member");
        }

        OwnerId = newOwnerId;
        UpdatedAt = DateTime.UtcNow;
    }
}
