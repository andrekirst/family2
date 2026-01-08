using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Family.Domain.Aggregates;

/// <summary>
/// Family aggregate root representing a family group.
/// NOTE: This is a pure domain aggregate. Member relationships are managed by the Auth module
/// through the User.FamilyId foreign key. The bidirectional navigation (Family.Members) is
/// handled at the persistence layer in the Auth module via EF Core shadow properties.
/// This maintains proper bounded context separation in the domain layer.
/// </summary>
public class Family : AggregateRoot<FamilyId>, ISoftDeletable
{
    /// <summary>
    /// Family name (e.g., "Smith Family").
    /// </summary>
    public FamilyName Name { get; private set; }

    /// <summary>
    /// ID of the user who owns this family.
    /// </summary>
    public UserId OwnerId { get; private set; }

    /// <summary>
    /// Soft delete timestamp (null if not deleted).
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    // Private constructor for EF Core
    private Family() : base(FamilyId.From(Guid.Empty))
    {
        Name = FamilyName.From("Placeholder"); // EF Core will set the actual value
        OwnerId = UserId.From(Guid.Empty); // EF Core will set the actual value
    }

    private Family(FamilyId id, FamilyName name, UserId ownerId) : base(id)
    {
        Name = name;
        OwnerId = ownerId;
    }

    /// <summary>
    /// Creates a new family with an owner.
    /// </summary>
    public static Family Create(FamilyName name, UserId ownerId)
    {
        var family = new Family(FamilyId.New(), name, ownerId);

        // Domain event
        // family.AddDomainEvent(new FamilyCreatedEvent(family.Id, family.Name, ownerId));

        return family;
    }

    /// <summary>
    /// Reconstitutes a Family aggregate from persisted data (e.g., from database or DTO).
    /// Used when rebuilding domain objects without re-applying business rules.
    /// </summary>
    /// <summary>
    /// Reconstitutes a Family aggregate from persisted data (e.g., from database or DTO).
    /// Used when rebuilding domain objects without re-applying business rules.
    /// </summary>
    public static Family Reconstitute(
        FamilyId id,
        FamilyName name,
        UserId ownerId,
        DateTime createdAt,
        DateTime updatedAt)
    {
        var family = new Family(id, name, ownerId)
        {
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
        
        return family;
    }

    /// <summary>
    /// Updates the family name.
    /// </summary>
    public void UpdateName(FamilyName newName)
    {
        Name = newName;
    }

    /// <summary>
    /// Transfers ownership to another member.
    /// </summary>
    public void TransferOwnership(UserId newOwnerId)
    {
        if (OwnerId == newOwnerId)
        {
            return;
        }

        var oldOwnerId = OwnerId;
        OwnerId = newOwnerId;

        // Domain event
        // AddDomainEvent(new FamilyOwnershipTransferredEvent(Id, oldOwnerId, newOwnerId));
    }

    /// <summary>
    /// Soft deletes the family.
    /// </summary>
    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;

        // Domain event
        // AddDomainEvent(new FamilyDeletedEvent(Id));
    }
}
