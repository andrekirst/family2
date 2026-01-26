using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Domain.Events;

/// <summary>
/// Domain event raised when a child user requests a profile change that requires approval.
/// </summary>
public sealed class ProfileChangeRequestedEvent(
    int eventVersion,
    ChangeRequestId requestId,
    UserProfileId profileId,
    UserId requestedBy,
    FamilyId familyId,
    string fieldName,
    string? oldValue,
    string newValue)
    : DomainEvent
{
    /// <summary>
    /// Event schema version for future evolution.
    /// </summary>
    public int EventVersion { get; } = eventVersion;

    /// <summary>
    /// Unique identifier for the change request.
    /// </summary>
    public ChangeRequestId RequestId { get; } = requestId;

    /// <summary>
    /// The profile being modified.
    /// </summary>
    public UserProfileId ProfileId { get; } = profileId;

    /// <summary>
    /// The user who requested the change (child user).
    /// </summary>
    public UserId RequestedBy { get; } = requestedBy;

    /// <summary>
    /// The family ID for routing notifications to parents/admins.
    /// </summary>
    public FamilyId FamilyId { get; } = familyId;

    /// <summary>
    /// The name of the field being changed.
    /// </summary>
    public string FieldName { get; } = fieldName;

    /// <summary>
    /// The current value of the field (null if not previously set).
    /// </summary>
    public string? OldValue { get; } = oldValue;

    /// <summary>
    /// The requested new value for the field.
    /// </summary>
    public string NewValue { get; } = newValue;
}
