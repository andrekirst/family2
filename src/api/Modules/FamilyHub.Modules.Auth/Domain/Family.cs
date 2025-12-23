using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain;

/// <summary>
/// Family aggregate root representing a family group.
/// </summary>
public class Family : AggregateRoot<FamilyId>
{
    private readonly List<UserFamily> _userFamilies = [];

    /// <summary>
    /// Family name (e.g., "Smith Family").
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// ID of the user who owns this family.
    /// </summary>
    public UserId OwnerId { get; private set; }

    /// <summary>
    /// When the family was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// When the family was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Soft delete timestamp (null if not deleted).
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// Family members.
    /// </summary>
    public IReadOnlyCollection<UserFamily> UserFamilies => _userFamilies.AsReadOnly();

    // Private constructor for EF Core
    private Family() : base(FamilyId.From(Guid.Empty))
    {
        Name = string.Empty; // EF Core will set the actual value
        OwnerId = UserId.From(Guid.Empty); // EF Core will set the actual value
    }

    private Family(FamilyId id, string name, UserId ownerId) : base(id)
    {
        Name = name;
        OwnerId = ownerId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new family with an owner.
    /// </summary>
    public static Family Create(string name, UserId ownerId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Family name cannot be empty.", nameof(name));
        }

        if (name.Length > 100)
        {
            throw new ArgumentException("Family name cannot exceed 100 characters.", nameof(name));
        }

        var family = new Family(FamilyId.New(), name.Trim(), ownerId);

        // Domain event
        // family.AddDomainEvent(new FamilyCreatedEvent(family.Id, family.Name, ownerId));

        return family;
    }

    /// <summary>
    /// Updates the family name.
    /// </summary>
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Family name cannot be empty.", nameof(newName));
        }

        if (newName.Length > 100)
        {
            throw new ArgumentException("Family name cannot exceed 100 characters.", nameof(newName));
        }

        Name = newName.Trim();
        UpdatedAt = DateTime.UtcNow;
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
        UpdatedAt = DateTime.UtcNow;

        // Domain event
        // AddDomainEvent(new FamilyOwnershipTransferredEvent(Id, oldOwnerId, newOwnerId));
    }

    /// <summary>
    /// Soft deletes the family.
    /// </summary>
    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // Domain event
        // AddDomainEvent(new FamilyDeletedEvent(Id));
    }

    /// <summary>
    /// Adds a member to the family.
    /// </summary>
    internal void AddMember(UserFamily userFamily)
    {
        _userFamilies.Add(userFamily);
    }
}
