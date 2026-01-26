using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Domain.Events;

/// <summary>
/// Domain event raised when a new user profile is created.
/// </summary>
public sealed class UserProfileCreatedEvent(
    int eventVersion,
    UserProfileId profileId,
    UserId userId,
    DisplayName displayName)
    : DomainEvent
{
    /// <summary>
    /// Event schema version for future evolution.
    /// </summary>
    public int EventVersion { get; } = eventVersion;

    /// <summary>
    /// Unique identifier for the created profile.
    /// </summary>
    public UserProfileId ProfileId { get; } = profileId;

    /// <summary>
    /// User ID that owns this profile.
    /// </summary>
    public UserId UserId { get; } = userId;

    /// <summary>
    /// Display name set for the profile.
    /// </summary>
    public DisplayName DisplayName { get; } = displayName;
}
