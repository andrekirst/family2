using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Application.Queries.GetPendingProfileChanges;

/// <summary>
/// Result of the GetPendingProfileChanges query.
/// Contains all pending change requests for the family.
/// </summary>
public sealed record GetPendingProfileChangesResult
{
    /// <summary>
    /// The list of pending change requests.
    /// </summary>
    public required IReadOnlyList<PendingChangeRequestItem> ChangeRequests { get; init; }

    /// <summary>
    /// Total count of pending change requests.
    /// </summary>
    public int TotalCount => ChangeRequests.Count;
}

/// <summary>
/// Represents a single pending change request item.
/// </summary>
public sealed record PendingChangeRequestItem
{
    /// <summary>
    /// The change request ID.
    /// </summary>
    public required ChangeRequestId RequestId { get; init; }

    /// <summary>
    /// The profile ID that the change applies to.
    /// </summary>
    public required UserProfileId ProfileId { get; init; }

    /// <summary>
    /// The user ID who requested the change.
    /// </summary>
    public required UserId RequestedBy { get; init; }

    /// <summary>
    /// The display name of the user who requested the change (for UI display).
    /// </summary>
    public string? RequestedByDisplayName { get; init; }

    /// <summary>
    /// The name of the field being changed.
    /// </summary>
    public required string FieldName { get; init; }

    /// <summary>
    /// The current/old value of the field.
    /// </summary>
    public string? OldValue { get; init; }

    /// <summary>
    /// The requested new value for the field.
    /// </summary>
    public required string NewValue { get; init; }

    /// <summary>
    /// When the change was requested.
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
