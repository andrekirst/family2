using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Domain.Entities;

/// <summary>
/// Family member entity representing the membership relationship between a User and a Family.
/// This is an entity owned by the Family aggregate, not a separate aggregate root.
/// Tracks the role and join date for each family member.
/// </summary>
public sealed class FamilyMember
{
    // Private parameterless constructor for EF Core
#pragma warning disable CS8618
    private FamilyMember() { }
#pragma warning restore CS8618

    /// <summary>
    /// Factory method to create a new family member.
    /// </summary>
    public static FamilyMember Create(FamilyId familyId, UserId userId, FamilyRole role)
    {
        return new FamilyMember
        {
            Id = FamilyMemberId.New(),
            FamilyId = familyId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    public FamilyMemberId Id { get; private set; }
    public FamilyId FamilyId { get; private set; }
    public UserId UserId { get; private set; }
    public FamilyRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>
    /// Optional per-family avatar override.
    /// If set, this takes precedence over the global User.AvatarId for this family context.
    /// </summary>
    public AvatarId? AvatarId { get; private set; }

    // Navigation properties
    public Family Family { get; private set; } = null!;
    public User User { get; private set; } = null!;

    /// <summary>
    /// Set a per-family avatar override for this member.
    /// </summary>
    public void SetFamilyAvatar(AvatarId avatarId)
    {
        AvatarId = avatarId;
    }

    /// <summary>
    /// Remove the per-family avatar override (falls back to global User.AvatarId).
    /// </summary>
    public void RemoveFamilyAvatar()
    {
        AvatarId = null;
    }
}
