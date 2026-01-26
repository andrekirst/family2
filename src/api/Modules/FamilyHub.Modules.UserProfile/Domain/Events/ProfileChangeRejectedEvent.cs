using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Domain.Events;

/// <summary>
/// Domain event raised when a profile change request is rejected by a parent/admin.
/// </summary>
public sealed class ProfileChangeRejectedEvent(
    int eventVersion,
    ChangeRequestId requestId,
    UserProfileId profileId,
    UserId rejectedBy,
    string fieldName,
    string reason)
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
    /// The profile for which the change was rejected.
    /// </summary>
    public UserProfileId ProfileId { get; } = profileId;

    /// <summary>
    /// The user who rejected the change (parent/admin).
    /// </summary>
    public UserId RejectedBy { get; } = rejectedBy;

    /// <summary>
    /// The name of the field that was rejected.
    /// </summary>
    public string FieldName { get; } = fieldName;

    /// <summary>
    /// The reason provided for rejecting the change.
    /// </summary>
    public string Reason { get; } = reason;
}
