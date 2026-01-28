namespace FamilyHub.SharedKernel.Presentation.GraphQL.Subscriptions;

/// <summary>
/// Payload for the <c>nodeChanged</c> subscription.
/// </summary>
/// <remarks>
/// <para>
/// This payload is published when an entity implementing the Node interface
/// is created, updated, or deleted. It provides:
/// <list type="bullet">
/// <item><description>The global ID of the affected node</description></item>
/// <item><description>The type of change (Created, Updated, Deleted)</description></item>
/// <item><description>The type name of the node for client-side type discrimination</description></item>
/// </list>
/// </para>
/// <para>
/// For <see cref="NodeChangeType.Deleted"/> events, clients should use the
/// <see cref="NodeId"/> to remove the entity from their local cache.
/// </para>
/// </remarks>
public sealed record NodeChangedPayload
{
    /// <summary>
    /// The global ID (Relay Node specification) of the affected entity.
    /// </summary>
    /// <example>VXNlclByb2ZpbGU6YTEyMzQ1NjctODlhYi1jZGVmLTAxMjMtNDU2Nzg5YWJjZGVm</example>
    public required string NodeId { get; init; }

    /// <summary>
    /// The type of change that occurred.
    /// </summary>
    public required NodeChangeType ChangeType { get; init; }

    /// <summary>
    /// The GraphQL type name of the node (e.g., "User", "UserProfile", "Family").
    /// </summary>
    /// <remarks>
    /// This allows clients to determine which cache entries to invalidate
    /// without needing to decode the global ID.
    /// </remarks>
    public required string TypeName { get; init; }

    /// <summary>
    /// The internal UUID of the entity (for clients that need the raw ID).
    /// </summary>
    public required Guid InternalId { get; init; }
}
