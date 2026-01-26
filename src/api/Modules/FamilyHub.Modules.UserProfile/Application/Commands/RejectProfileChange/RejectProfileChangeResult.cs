using FamilyHub.Modules.UserProfile.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Application.Commands.RejectProfileChange;

/// <summary>
/// Result of rejecting a profile change request.
/// </summary>
public sealed record RejectProfileChangeResult
{
    /// <summary>
    /// The ID of the rejected change request.
    /// </summary>
    public required ChangeRequestId RequestId { get; init; }

    /// <summary>
    /// The profile for which the change was rejected.
    /// </summary>
    public required UserProfileId ProfileId { get; init; }

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
