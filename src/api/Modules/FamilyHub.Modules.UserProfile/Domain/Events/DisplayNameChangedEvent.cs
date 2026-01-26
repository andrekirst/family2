using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Domain.Events;

/// <summary>
/// Domain event raised when a user's display name is changed.
/// Triggers event chain: Cache invalidation and real-time UI updates.
/// Note: FamilyId is queried by handlers via IUserLookupService for cross-module routing.
/// </summary>
public sealed class DisplayNameChangedEvent(
    int eventVersion,
    UserProfileId profileId,
    UserId userId,
    DisplayName oldDisplayName,
    DisplayName newDisplayName)
    : DomainEvent
{
    /// <summary>
    /// Event schema version for future evolution.
    /// </summary>
    public int EventVersion { get; } = eventVersion;

    /// <summary>
    /// Unique identifier for the profile.
    /// </summary>
    public UserProfileId ProfileId { get; } = profileId;

    /// <summary>
    /// User ID that owns this profile.
    /// </summary>
    public UserId UserId { get; } = userId;

    /// <summary>
    /// The previous display name before the change.
    /// </summary>
    public DisplayName OldDisplayName { get; } = oldDisplayName;

    /// <summary>
    /// The new display name after the change.
    /// </summary>
    public DisplayName NewDisplayName { get; } = newDisplayName;
}
