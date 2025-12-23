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
    /// Who invited this user (null for owner).
    /// </summary>
    public UserId? InvitedBy { get; private set; }

    /// <summary>
    /// When the user joined the family.
    /// </summary>
    public DateTime JoinedAt { get; private set; }

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
        UserId = UserId.From(Guid.Empty); // EF Core will set the actual value
        FamilyId = FamilyId.From(Guid.Empty); // EF Core will set the actual value
        Role = UserRole.Member; // EF Core will set the actual value
    }

    private UserFamily(
        UserId userId,
        FamilyId familyId,
        UserRole role,
        UserId? invitedBy) : base(Guid.NewGuid())
    {
        UserId = userId;
        FamilyId = familyId;
        Role = role;
        IsActive = true;
        InvitedBy = invitedBy;
        JoinedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new family membership for the owner.
    /// </summary>
    public static UserFamily CreateOwnerMembership(UserId userId, FamilyId familyId)
    {
        return new UserFamily(userId, familyId, UserRole.Owner, invitedBy: null);
    }

    /// <summary>
    /// Creates a new family membership for an invited member.
    /// </summary>
    public static UserFamily CreateMembership(
        UserId userId,
        FamilyId familyId,
        UserRole role,
        UserId invitedBy)
    {
        if (role == UserRole.Owner)
        {
            throw new InvalidOperationException("Use CreateOwnerMembership for owner role.");
        }

        return new UserFamily(userId, familyId, role, invitedBy);
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
}
