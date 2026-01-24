namespace FamilyHub.Modules.Auth.Application.DTOs.Subscriptions;

/// <summary>
/// Subscription payload for family members changes.
/// Published when a member joins, leaves, or is updated.
/// </summary>
public sealed record FamilyMembersChangedPayload
{
    /// <summary>
    /// ID of the family where the change occurred.
    /// </summary>
    public required Guid FamilyId { get; init; }

    /// <summary>
    /// Type of change (ADDED, UPDATED, REMOVED).
    /// </summary>
    public required ChangeType ChangeType { get; init; }

    /// <summary>
    /// The family member that changed.
    /// Null for REMOVED events (member data already deleted).
    /// </summary>
    public FamilyMemberDto? Member { get; init; }
}
