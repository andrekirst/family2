using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Domain.Events;

/// <summary>
/// Domain event raised when a user's birthday is set or changed.
/// Triggers event chain: Calendar module creates recurring birthday event.
/// Note: FamilyId is queried by handlers via IUserLookupService for cross-module routing.
/// </summary>
public sealed class BirthdaySetEvent(
    int eventVersion,
    UserProfileId profileId,
    UserId userId,
    Birthday birthday,
    DisplayName displayName)
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
    /// The birthday that was set.
    /// </summary>
    public Birthday Birthday { get; } = birthday;

    /// <summary>
    /// Display name for calendar event creation (e.g., "John's Birthday").
    /// </summary>
    public DisplayName DisplayName { get; } = displayName;
}
