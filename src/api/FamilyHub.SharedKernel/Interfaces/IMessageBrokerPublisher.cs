namespace FamilyHub.SharedKernel.Interfaces;

/// <summary>
/// Interface for publishing messages to a message broker.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts message broker publishing operations to enable:
/// - Dependency injection and testability
/// - Consistent retry and error handling
/// - Cross-module event publishing
/// - Easy swapping of message broker implementations (RabbitMQ, Azure Service Bus, Kafka, etc.)
/// </para>
/// <para>
/// Messages are published as JSON payloads to topic exchanges.
/// Failed messages are routed to a dead letter queue after retry exhaustion.
/// </para>
/// </remarks>
public interface IMessageBrokerPublisher
{
    /// <summary>
    /// Publishes a message to a specific exchange with routing key.
    /// </summary>
    /// <param name="exchange">The exchange to publish to (e.g., "family-hub.events").</param>
    /// <param name="routingKey">The routing key for message routing (e.g., event type name).</param>
    /// <param name="message">The message payload (JSON format).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the message is confirmed by the broker.</returns>
    /// <exception cref="ArgumentException">Thrown when exchange, routingKey, or message is null or empty.</exception>
    /// <remarks>
    /// Throws BrokerUnreachableException when unable to connect to the message broker after all retry attempts.
    /// </remarks>
    Task PublishAsync(
        string exchange,
        string routingKey,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a strongly-typed message to a specific exchange with routing key.
    /// The message is automatically serialized to JSON.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to publish.</typeparam>
    /// <param name="exchange">The exchange to publish to (e.g., "family-hub.events").</param>
    /// <param name="routingKey">The routing key for message routing (e.g., event type name).</param>
    /// <param name="message">The message object to serialize and publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the message is confirmed by the broker.</returns>
    /// <exception cref="ArgumentException">Thrown when exchange or routingKey is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when message is null.</exception>
    /// <remarks>
    /// <para>
    /// This method provides a type-safe alternative to the string-based PublishAsync method.
    /// It handles JSON serialization internally using System.Text.Json with camelCase naming policy.
    /// </para>
    /// <para>
    /// Throws BrokerUnreachableException when unable to connect to the message broker after all retry attempts.
    /// </para>
    /// </remarks>
    Task PublishAsync<TMessage>(
        string exchange,
        string routingKey,
        TMessage message,
        CancellationToken cancellationToken = default)
        where TMessage : class;
}
