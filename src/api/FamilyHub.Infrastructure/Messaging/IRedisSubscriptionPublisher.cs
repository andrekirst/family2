namespace FamilyHub.Infrastructure.Messaging;

/// <summary>
/// Publisher for GraphQL subscription messages via Redis PubSub.
/// Used for real-time updates to connected WebSocket clients.
/// </summary>
/// <remarks>
/// <para>
/// This publisher uses Hot Chocolate's <c>ITopicEventSender</c> to send messages
/// to Redis PubSub topics, which are then delivered to subscribed GraphQL clients
/// via WebSocket connections.
/// </para>
/// <para>
/// <strong>Key Differences from <see cref="FamilyHub.SharedKernel.Interfaces.IMessageBrokerPublisher"/>:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>RabbitMQ (IMessageBrokerPublisher): Durable messages for cross-module communication</description></item>
/// <item><description>Redis (IRedisSubscriptionPublisher): Ephemeral messages for real-time subscriptions</description></item>
/// </list>
/// <para>
/// <strong>Usage Pattern:</strong>
/// </para>
/// <code>
/// public class InvitationAcceptedSubscriptionHandler
///     : INotificationHandler&lt;InvitationAcceptedEvent&gt;
/// {
///     private readonly IRedisSubscriptionPublisher _publisher;
///
///     public async Task Handle(InvitationAcceptedEvent notification, CancellationToken cancellationToken)
///     {
///         var payload = new FamilyMembersChangedPayload(
///             FamilyId: notification.FamilyId,
///             ChangeType: MemberChangeType.Added,
///             Member: notification.Member
///         );
///
///         await _publisher.PublishAsync(
///             $"family-members-changed:{notification.FamilyId}",
///             payload,
///             cancellationToken
///         );
///     }
/// }
/// </code>
/// <para>
/// <strong>Topic Naming Convention:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><c>family-members-changed:{familyId}</c> - Family member updates (ADDED, UPDATED, REMOVED)</description></item>
/// <item><description><c>pending-invitations-changed:{familyId}</c> - Invitation updates (ADDED, UPDATED, REMOVED)</description></item>
/// </list>
/// </remarks>
public interface IRedisSubscriptionPublisher
{
    /// <summary>
    /// Publishes a message to a GraphQL subscription topic via Redis PubSub.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message payload (subscription payload type).</typeparam>
    /// <param name="topicName">
    /// The subscription topic name. Supports parameterized topics (e.g., "family-members-changed:{familyId}").
    /// Must match the topic defined in the subscription resolver's [Topic] attribute.
    /// </param>
    /// <param name="message">The message payload to send to subscribers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the message is sent to Redis PubSub.</returns>
    /// <exception cref="ArgumentException">Thrown when topicName is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when message is null.</exception>
    /// <remarks>
    /// <para>
    /// This method sends messages to Redis PubSub, which Hot Chocolate then delivers to
    /// subscribed GraphQL clients via WebSocket connections. Messages are ephemeral and
    /// not persisted (best-effort delivery).
    /// </para>
    /// <para>
    /// Errors are logged but not thrown - subscription publishing is best-effort to avoid
    /// impacting the primary operation (e.g., accepting an invitation).
    /// </para>
    /// </remarks>
    Task PublishAsync<TMessage>(
        string topicName,
        TMessage message,
        CancellationToken cancellationToken = default)
        where TMessage : class;
}
