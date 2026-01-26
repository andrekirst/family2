namespace FamilyHub.Modules.UserProfile.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL output type for a profile change request.
/// </summary>
public sealed record ProfileChangeRequestDto
{
    /// <summary>
    /// The change request ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The profile ID that the change applies to.
    /// </summary>
    public required Guid ProfileId { get; init; }

    /// <summary>
    /// The user ID who requested the change.
    /// </summary>
    public required Guid RequestedBy { get; init; }

    /// <summary>
    /// The display name of the user who requested the change.
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
    /// The status of the change request.
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// When the change was requested.
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}

/// <summary>
/// GraphQL output type for pending profile change (child's view).
/// </summary>
public sealed record MyPendingChangeDto
{
    /// <summary>
    /// The change request ID.
    /// </summary>
    public required Guid Id { get; init; }

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
/// GraphQL output type for a rejected change (child's view).
/// </summary>
public sealed record MyRejectedChangeDto
{
    /// <summary>
    /// The change request ID.
    /// </summary>
    public required Guid Id { get; init; }

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
    /// When the change was rejected.
    /// </summary>
    public required DateTime RejectedAt { get; init; }
}

/// <summary>
/// GraphQL output type for pending changes result (parent's view).
/// </summary>
public sealed record PendingProfileChangesDto
{
    /// <summary>
    /// The list of pending change requests.
    /// </summary>
    public required IReadOnlyList<ProfileChangeRequestDto> ChangeRequests { get; init; }

    /// <summary>
    /// Total count of pending change requests.
    /// </summary>
    public int TotalCount => ChangeRequests.Count;
}

/// <summary>
/// GraphQL output type for my pending changes result (child's view).
/// </summary>
public sealed record MyPendingChangesDto
{
    /// <summary>
    /// The list of pending change requests.
    /// </summary>
    public required IReadOnlyList<MyPendingChangeDto> PendingChanges { get; init; }

    /// <summary>
    /// The list of recently rejected change requests.
    /// </summary>
    public required IReadOnlyList<MyRejectedChangeDto> RecentlyRejected { get; init; }

    /// <summary>
    /// Whether the user has any pending changes.
    /// </summary>
    public bool HasPendingChanges => PendingChanges.Count > 0;
}

/// <summary>
/// GraphQL payload for approving a profile change.
/// </summary>
public sealed record ApproveProfileChangePayload
{
    /// <summary>
    /// The change request ID.
    /// </summary>
    public required Guid RequestId { get; init; }

    /// <summary>
    /// The profile ID the change was applied to.
    /// </summary>
    public required Guid ProfileId { get; init; }

    /// <summary>
    /// The field that was changed.
    /// </summary>
    public required string FieldName { get; init; }

    /// <summary>
    /// The new value that was applied.
    /// </summary>
    public required string NewValue { get; init; }

    /// <summary>
    /// When the change was approved.
    /// </summary>
    public required DateTime ApprovedAt { get; init; }
}

/// <summary>
/// GraphQL payload for rejecting a profile change.
/// </summary>
public sealed record RejectProfileChangePayload
{
    /// <summary>
    /// The change request ID.
    /// </summary>
    public required Guid RequestId { get; init; }

    /// <summary>
    /// The profile ID the change was rejected for.
    /// </summary>
    public required Guid ProfileId { get; init; }

    /// <summary>
    /// The field that was rejected.
    /// </summary>
    public required string FieldName { get; init; }

    /// <summary>
    /// The reason provided for rejection.
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    /// When the change was rejected.
    /// </summary>
    public required DateTime RejectedAt { get; init; }
}
