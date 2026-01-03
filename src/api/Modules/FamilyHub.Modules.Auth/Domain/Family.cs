using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain;

/// <summary>
/// Family aggregate root representing a family group.
/// </summary>
public class Family : AggregateRoot<FamilyId>, ISoftDeletable
{
    private readonly List<User> _members = [];

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

    /// <summary>
    /// Family members (navigation property for EF Core).
    /// </summary>
    public IReadOnlyCollection<User> Members => _members.AsReadOnly();

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

    /// <summary>
    /// Gets the number of members in this family.
    /// </summary>
    public int GetMemberCount() => _members.Count;
}
