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
    /// Factory method to create a new Family aggregate.
    /// Raises FamilyCreatedEvent for downstream processing.
    /// </summary>
    /// <param name="name">Family name (e.g., "Smith Family")</param>
    /// <param name="ownerId">User ID of the family creator/owner</param>
    /// <returns>New Family aggregate with domain event raised</returns>
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
            family.CreatedAt
        ));

        return family;
    }

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
    /// Navigation property to all family members (User entities via FamilyId FK)
    /// </summary>
    public ICollection<User> Members { get; private set; } = new List<User>();

    /// <summary>
    /// Navigation property to family membership records with roles
    /// </summary>
    public ICollection<FamilyMember> FamilyMembers { get; private set; } = new List<FamilyMember>();
}
