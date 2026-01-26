using FamilyHub.Modules.UserProfile.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Application.Commands.ApproveProfileChange;

/// <summary>
/// Result of approving a profile change request.
/// </summary>
public sealed record ApproveProfileChangeResult
{
    /// <summary>
    /// The ID of the approved change request.
    /// </summary>
    public required ChangeRequestId RequestId { get; init; }

    /// <summary>
    /// The profile that was updated.
    /// </summary>
    public required UserProfileId ProfileId { get; init; }

    /// <summary>
    /// The field that was changed.
    /// </summary>
    public required string FieldName { get; init; }

    /// <summary>
    /// The new value that was applied to the profile.
    /// </summary>
    public required string NewValue { get; init; }

    /// <summary>
    /// When the change was approved.
    /// </summary>
    public required DateTime ApprovedAt { get; init; }
}
