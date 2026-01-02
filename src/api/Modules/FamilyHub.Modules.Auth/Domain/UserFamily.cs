using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.Modules.Auth.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain;

/// <summary>
/// Represents a user's membership in a family with a specific role.
/// </summary>
public class UserFamily : Entity<Guid>
{
    /// <summary>
    /// User ID.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Family ID.
    /// </summary>
    public FamilyId FamilyId { get; private set; }

    /// <summary>
    /// User's role in this family.
    /// </summary>
    public UserRole Role { get; private set; }

    /// <summary>
    /// Whether this membership is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Whether this is the user's currently active family.
    /// Only ONE UserFamily per user should have this flag set to true.
    /// </summary>
    public bool IsCurrentFamily { get; private set; }

    /// <summary>
    /// When the membership was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Navigation property to User.
    /// </summary>
    public User User { get; private set; } = null!;

    /// <summary>
    /// Navigation property to Family.
    /// </summary>
    public Family Family { get; private set; } = null!;

    // Private constructor for EF Core
    private UserFamily() : base(Guid.Empty)
    {
        // EF Core will set actual values via reflection after instantiation
        UserId = UserId.From(Guid.Empty);
        FamilyId = FamilyId.From(Guid.Empty);
        Role = UserRole.Member;
    }

    private UserFamily(
        UserId userId,
        FamilyId familyId,
        UserRole role,
        bool isCurrentFamily = false) : base(Guid.NewGuid())
    {
        UserId = userId;
        FamilyId = familyId;
        Role = role;
        IsActive = true;
        IsCurrentFamily = isCurrentFamily;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new family membership for the owner.
    /// Owner membership is automatically marked as current family.
    /// </summary>
    public static UserFamily CreateOwnerMembership(UserId userId, FamilyId familyId)
    {
        return new UserFamily(userId, familyId, UserRole.Owner, isCurrentFamily: true);
    }

    /// <summary>
    /// Creates a new family membership for an invited member.
    /// </summary>
    public static UserFamily CreateMembership(
        UserId userId,
        FamilyId familyId,
        UserRole role,
        bool isCurrentFamily = false)
    {
        if (role == UserRole.Owner)
        {
            throw new InvalidOperationException("Use CreateOwnerMembership for owner role.");
        }

        return new UserFamily(userId, familyId, role, isCurrentFamily);
    }

    /// <summary>
    /// Updates the user's role in the family.
    /// </summary>
    public void UpdateRole(UserRole newRole)
    {
        if (Role == UserRole.Owner && newRole != UserRole.Owner)
        {
            throw new InvalidOperationException("Cannot change role from owner. Transfer ownership first.");
        }

        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the membership (user leaves family).
    /// </summary>
    public void Deactivate()
    {
        if (Role == UserRole.Owner)
        {
            throw new InvalidOperationException("Owner cannot leave the family. Transfer ownership first.");
        }

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivates the membership.
    /// </summary>
    public void Reactivate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks this family as the user's current active family.
    /// NOTE: Caller is responsible for ensuring only ONE UserFamily per user has this flag set.
    /// </summary>
    public void MarkAsCurrentFamily()
    {
        IsCurrentFamily = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Unmarks this family as the user's current active family.
    /// </summary>
    public void UnmarkAsCurrentFamily()
    {
        IsCurrentFamily = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
