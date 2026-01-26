using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain;

namespace FamilyHub.Modules.UserProfile.Domain.Events;

/// <summary>
/// Domain event raised when a user profile is updated.
/// </summary>
public sealed class UserProfileUpdatedEvent(
    int eventVersion,
    UserProfileId profileId,
    string updatedField)
    : DomainEvent
{
    /// <summary>
    /// Event schema version for future evolution.
    /// </summary>
    public int EventVersion { get; } = eventVersion;

    /// <summary>
    /// Unique identifier for the updated profile.
    /// </summary>
    public UserProfileId ProfileId { get; } = profileId;

    /// <summary>
    /// Name of the field that was updated.
    /// Used for targeted event handling.
    /// </summary>
    public string UpdatedField { get; } = updatedField;
}
