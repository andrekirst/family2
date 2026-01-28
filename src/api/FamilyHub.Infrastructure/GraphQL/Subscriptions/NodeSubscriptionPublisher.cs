using FamilyHub.Infrastructure.Messaging;
using FamilyHub.SharedKernel.Presentation.GraphQL.Relay;
using FamilyHub.SharedKernel.Presentation.GraphQL.Subscriptions;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Infrastructure.GraphQL.Subscriptions;

/// <summary>
/// Default implementation of <see cref="INodeSubscriptionPublisher"/> using Redis PubSub.
/// </summary>
/// <remarks>
/// <para>
/// This implementation publishes node change events to two topics:
/// <list type="bullet">
/// <item><description><c>node-changed:{globalId}</c> - For subscribers watching a specific entity</description></item>
/// <item><description><c>node-type-changed:{typeName}</c> - For subscribers watching all entities of a type</description></item>
/// </list>
/// </para>
/// <para>
/// Both topics receive the same <see cref="NodeChangedPayload"/>, allowing clients to
/// choose their subscription granularity.
/// </para>
/// </remarks>
public sealed class NodeSubscriptionPublisher : INodeSubscriptionPublisher
{
    private readonly IRedisSubscriptionPublisher _redisPublisher;
    private readonly ILogger<NodeSubscriptionPublisher> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NodeSubscriptionPublisher"/> class.
    /// </summary>
    /// <param name="redisPublisher">The Redis subscription publisher.</param>
    /// <param name="logger">The logger.</param>
    public NodeSubscriptionPublisher(
        IRedisSubscriptionPublisher redisPublisher,
        ILogger<NodeSubscriptionPublisher> logger)
    {
        _redisPublisher = redisPublisher ?? throw new ArgumentNullException(nameof(redisPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task PublishNodeCreatedAsync<TNode>(Guid internalId, CancellationToken cancellationToken = default)
        where TNode : class
    {
        var typeName = GetTypeName<TNode>();
        return PublishNodeChangedAsync(typeName, internalId, NodeChangeType.Created, cancellationToken);
    }

    /// <inheritdoc />
    public Task PublishNodeUpdatedAsync<TNode>(Guid internalId, CancellationToken cancellationToken = default)
        where TNode : class
    {
        var typeName = GetTypeName<TNode>();
        return PublishNodeChangedAsync(typeName, internalId, NodeChangeType.Updated, cancellationToken);
    }

    /// <inheritdoc />
    public Task PublishNodeDeletedAsync<TNode>(Guid internalId, CancellationToken cancellationToken = default)
        where TNode : class
    {
        var typeName = GetTypeName<TNode>();
        return PublishNodeChangedAsync(typeName, internalId, NodeChangeType.Deleted, cancellationToken);
    }

    /// <inheritdoc />
    public async Task PublishNodeChangedAsync(
        string typeName,
        Guid internalId,
        NodeChangeType changeType,
        CancellationToken cancellationToken = default)
    {
        var globalId = GlobalIdSerializer.Serialize(typeName, internalId);

        var payload = new NodeChangedPayload
        {
            NodeId = globalId,
            ChangeType = changeType,
            TypeName = typeName,
            InternalId = internalId
        };

        // Publish to entity-specific topic
        var entityTopic = $"node-changed:{globalId}";
        await _redisPublisher.PublishAsync(entityTopic, payload, cancellationToken);

        // Publish to type-level topic
        var typeTopic = $"node-type-changed:{typeName}";
        await _redisPublisher.PublishAsync(typeTopic, payload, cancellationToken);

        _logger.LogDebug(
            "Published {ChangeType} event for {TypeName}:{InternalId} (globalId: {GlobalId})",
            changeType,
            typeName,
            internalId,
            globalId);
    }

    /// <summary>
    /// Gets the GraphQL type name for an entity type.
    /// </summary>
    /// <remarks>
    /// Uses simple class name extraction. For types with "Aggregate" suffix,
    /// the suffix is removed to match GraphQL type naming conventions.
    /// </remarks>
    private static string GetTypeName<TNode>()
    {
        var typeName = typeof(TNode).Name;

        // Remove common suffixes to match GraphQL type names
        if (typeName.EndsWith("Aggregate", StringComparison.Ordinal))
        {
            typeName = typeName[..^9]; // Remove "Aggregate"
        }

        return typeName;
    }
}
