namespace FamilyHub.SharedKernel.Presentation.GraphQL.Subscriptions;

/// <summary>
/// Enumeration of possible change types for entity subscriptions.
/// </summary>
/// <remarks>
/// Used with the <c>nodeChanged</c> subscription to indicate what type of
/// change occurred to the subscribed entity.
/// </remarks>
public enum NodeChangeType
{
    /// <summary>
    /// A new entity was created.
    /// </summary>
    Created,

    /// <summary>
    /// An existing entity was updated.
    /// </summary>
    Updated,

    /// <summary>
    /// An entity was deleted.
    /// </summary>
    Deleted
}
