using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Application.Queries.GetMyPendingChanges;

/// <summary>
/// Result of the GetMyPendingChanges query.
/// Contains the current user's pending and recently reviewed change requests.
/// </summary>
public sealed record GetMyPendingChangesResult
{
    /// <summary>
    /// The list of pending change requests.
    /// </summary>
    public required IReadOnlyList<MyPendingChangeItem> PendingChanges { get; init; }

    /// <summary>
    /// The list of recently rejected change requests (for user feedback).
    /// </summary>
    public required IReadOnlyList<MyRejectedChangeItem> RecentlyRejected { get; init; }

    /// <summary>
    /// Whether the user has any pending changes awaiting approval.
    /// </summary>
    public bool HasPendingChanges => PendingChanges.Count > 0;
}

/// <summary>
/// Represents a pending change request for the current user.
/// </summary>
public sealed record MyPendingChangeItem
{
    /// <summary>
    /// The change request ID.
    /// </summary>
    public required ChangeRequestId RequestId { get; init; }

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

/// <summary>
/// Represents a recently rejected change request for the current user.
/// </summary>
public sealed record MyRejectedChangeItem
{
    /// <summary>
    /// The change request ID.
    /// </summary>
    public required ChangeRequestId RequestId { get; init; }

    /// <summary>
    /// The name of the field that was rejected.
    /// </summary>
    public required string FieldName { get; init; }

    /// <summary>
    /// The value that was requested.
    /// </summary>
    public required string RequestedValue { get; init; }

    /// <summary>
    /// The reason provided for rejection.
    /// </summary>
    public required string RejectionReason { get; init; }

    /// <summary>
    /// The user ID of the reviewer who rejected the change.
    /// </summary>
    public required UserId RejectedBy { get; init; }

    /// <summary>
    /// When the change was rejected.
    /// </summary>
    public required DateTime RejectedAt { get; init; }
}
