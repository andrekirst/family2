using FamilyHub.Modules.UserProfile.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Application.Commands.UpdateUserProfile;

/// <summary>
/// Result of the UpdateUserProfile command.
/// </summary>
public sealed record UpdateUserProfileResult
{
    /// <summary>
    /// The profile ID.
    /// </summary>
    public required UserProfileId ProfileId { get; init; }

    /// <summary>
    /// The updated display name.
    /// </summary>
    public required DisplayName DisplayName { get; init; }

    /// <summary>
    /// When the profile was last updated.
    /// </summary>
    public required DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Whether a new profile was created (true) or existing profile was updated (false).
    /// </summary>
    public required bool IsNewProfile { get; init; }

    /// <summary>
    /// Whether the changes require approval (true for child users).
    /// When true, changes were not applied directly but submitted for parent approval.
    /// </summary>
    public bool RequiresApproval { get; init; }

    /// <summary>
    /// Number of pending changes awaiting approval.
    /// Only populated when RequiresApproval is true.
    /// </summary>
    public int PendingChangesCount { get; init; }
}
