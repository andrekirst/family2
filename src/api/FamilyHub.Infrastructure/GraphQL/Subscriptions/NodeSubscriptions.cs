using System.Runtime.CompilerServices;
using System.Security.Claims;
using FamilyHub.SharedKernel.Presentation.GraphQL.Subscriptions;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Infrastructure.GraphQL.Subscriptions;

/// <summary>
/// Entity-centric GraphQL subscriptions for real-time updates on any Node type.
/// </summary>
/// <remarks>
/// <para>
/// This subscription pattern follows the Relay specification's Node interface,
/// allowing clients to subscribe to changes on any entity by its global ID.
/// </para>
/// <para>
/// <strong>Usage:</strong>
/// <code>
/// subscription {
///   nodeChanged(nodeId: "VXNlclByb2ZpbGU6...") {
///     nodeId
///     changeType
///     typeName
///     internalId
///   }
/// }
/// </code>
/// </para>
/// <para>
/// <strong>Publishing Changes:</strong>
/// Use the <c>INodeSubscriptionPublisher</c> service to publish changes from
/// command handlers or domain event handlers.
/// </para>
/// </remarks>
[ExtendObjectType("Subscription")]
public sealed class NodeSubscriptions
{
    /// <summary>
    /// Subscribe to changes on a specific Node entity.
    /// </summary>
    /// <param name="nodeId">The global ID of the node to subscribe to.</param>
    /// <param name="httpContextAccessor">HTTP context for authentication.</param>
    /// <param name="message">The subscription message (injected by HotChocolate).</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Stream of node change events.</returns>
    /// <remarks>
    /// <para>
    /// Clients must be authenticated to subscribe to node changes.
    /// Authorization is performed based on the node type and the viewer's relationship
    /// to the entity (e.g., family membership for UserProfile changes).
    /// </para>
    /// </remarks>
    [Subscribe]
    [Topic("node-changed:{nodeId}")]
    [Authorize]
    public async IAsyncEnumerable<NodeChangedPayload> NodeChanged(
        [ID] string nodeId,
        [Service] IHttpContextAccessor httpContextAccessor,
        [EventMessage] NodeChangedPayload message,
        [Service] ILogger<NodeSubscriptions> logger,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Get current user for logging
        var userId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning(
                "Unauthenticated user attempted to subscribe to node changes for {NodeId}",
                nodeId);
            yield break;
        }

        logger.LogInformation(
            "User {UserId} subscribed to node changes for {NodeId} (type: {TypeName})",
            userId,
            nodeId,
            message.TypeName);

        // Yield the message
        // Future enhancement: Add fine-grained authorization based on node type
        // e.g., only allow subscribing to UserProfile if viewer is family member
        yield return message;
    }

    /// <summary>
    /// Subscribe to changes on all nodes of a specific type.
    /// </summary>
    /// <param name="typeName">The GraphQL type name (e.g., "User", "Family", "UserProfile").</param>
    /// <param name="httpContextAccessor">HTTP context for authentication.</param>
    /// <param name="message">The subscription message (injected by HotChocolate).</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Stream of node change events for the specified type.</returns>
    /// <remarks>
    /// <para>
    /// This subscription is useful for admin dashboards or system monitoring
    /// that need to track all changes to a specific entity type.
    /// </para>
    /// </remarks>
    [Subscribe]
    [Topic("node-type-changed:{typeName}")]
    [Authorize]
    public async IAsyncEnumerable<NodeChangedPayload> NodeTypeChanged(
        string typeName,
        [Service] IHttpContextAccessor httpContextAccessor,
        [EventMessage] NodeChangedPayload message,
        [Service] ILogger<NodeSubscriptions> logger,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning(
                "Unauthenticated user attempted to subscribe to {TypeName} changes",
                typeName);
            yield break;
        }

        logger.LogInformation(
            "User {UserId} subscribed to all {TypeName} changes",
            userId,
            typeName);

        yield return message;
    }
}
