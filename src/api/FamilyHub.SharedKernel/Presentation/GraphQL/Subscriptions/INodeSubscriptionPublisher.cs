namespace FamilyHub.SharedKernel.Presentation.GraphQL.Subscriptions;

/// <summary>
/// Service for publishing node change events to GraphQL subscriptions.
/// </summary>
/// <remarks>
/// <para>
/// This service provides a convenient way for command handlers and domain event
/// handlers to publish real-time updates when entities are created, updated, or deleted.
/// </para>
/// <para>
/// <strong>Usage in Command Handlers:</strong>
/// <code>
/// // After saving changes
/// await _nodePublisher.PublishNodeCreatedAsync&lt;UserProfile&gt;(
///     profile.Id.Value,
///     cancellationToken);
/// </code>
/// </para>
/// <para>
/// <strong>Usage in Domain Event Handlers:</strong>
/// <code>
/// public class UserProfileUpdatedEventHandler : INotificationHandler&lt;UserProfileUpdatedEvent&gt;
/// {
///     public async Task Handle(UserProfileUpdatedEvent notification, CancellationToken ct)
///     {
///         await _nodePublisher.PublishNodeUpdatedAsync&lt;UserProfile&gt;(
///             notification.ProfileId.Value, ct);
///     }
/// }
/// </code>
/// </para>
/// </remarks>
public interface INodeSubscriptionPublisher
{
    /// <summary>
    /// Publishes a node created event.
    /// </summary>
    /// <typeparam name="TNode">The type name to use in the payload (e.g., "UserProfile").</typeparam>
    /// <param name="internalId">The internal UUID of the created entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishNodeCreatedAsync<TNode>(Guid internalId, CancellationToken cancellationToken = default)
        where TNode : class;

    /// <summary>
    /// Publishes a node updated event.
    /// </summary>
    /// <typeparam name="TNode">The type name to use in the payload.</typeparam>
    /// <param name="internalId">The internal UUID of the updated entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishNodeUpdatedAsync<TNode>(Guid internalId, CancellationToken cancellationToken = default)
        where TNode : class;

    /// <summary>
    /// Publishes a node deleted event.
    /// </summary>
    /// <typeparam name="TNode">The type name to use in the payload.</typeparam>
    /// <param name="internalId">The internal UUID of the deleted entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishNodeDeletedAsync<TNode>(Guid internalId, CancellationToken cancellationToken = default)
        where TNode : class;

    /// <summary>
    /// Publishes a node change event with explicit type name.
    /// </summary>
    /// <param name="typeName">The GraphQL type name.</param>
    /// <param name="internalId">The internal UUID of the entity.</param>
    /// <param name="changeType">The type of change.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishNodeChangedAsync(
        string typeName,
        Guid internalId,
        NodeChangeType changeType,
        CancellationToken cancellationToken = default);
}
