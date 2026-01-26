using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Domain.Events;

/// <summary>
/// Domain event raised when a profile change request is approved by a parent/admin.
/// </summary>
public sealed class ProfileChangeApprovedEvent(
    int eventVersion,
    ChangeRequestId requestId,
    UserProfileId profileId,
    UserId approvedBy,
    string fieldName,
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
    /// The profile that was modified.
    /// </summary>
    public UserProfileId ProfileId { get; } = profileId;

    /// <summary>
    /// The user who approved the change (parent/admin).
    /// </summary>
    public UserId ApprovedBy { get; } = approvedBy;

    /// <summary>
    /// The name of the field that was changed.
    /// </summary>
    public string FieldName { get; } = fieldName;

    /// <summary>
    /// The new value that was applied to the profile.
    /// </summary>
    public string NewValue { get; } = newValue;
}
