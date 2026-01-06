namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL enum for subscription change types.
/// Indicates what type of change occurred in real-time updates.
/// </summary>
public enum ChangeType
{
    /// <summary>
    /// A new item was added (e.g., new family member, new invitation).
    /// </summary>
    ADDED,

    /// <summary>
    /// An existing item was updated (e.g., role changed, invitation resent).
    /// </summary>
    UPDATED,

    /// <summary>
    /// An existing item was removed (e.g., member left, invitation canceled).
    /// </summary>
    REMOVED
}
