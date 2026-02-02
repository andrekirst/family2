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
    public User Owner { get; private set; } = null!;

    /// <summary>
    /// Navigation property to all family members
    /// </summary>
    public ICollection<User> Members { get; private set; } = new List<User>();

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

    // Future domain methods (add when implementing corresponding features):
    // - AddMember(User user) - When implementing "Invite family member" feature
    // - RemoveMember(User user) - When implementing "Remove member" feature
    // - Rename(FamilyName newName) - When implementing "Rename family" feature
    // - TransferOwnership(UserId newOwnerId) - When implementing "Transfer ownership" feature
}
