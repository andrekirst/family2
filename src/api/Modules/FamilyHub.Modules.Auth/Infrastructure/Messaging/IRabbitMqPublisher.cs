namespace FamilyHub.Modules.Auth.Infrastructure.Messaging;

/// <summary>
/// Interface for publishing messages to RabbitMQ.
/// </summary>
public interface IRabbitMqPublisher
{
    /// <summary>
    /// Publishes a message to a specific exchange with routing key.
    /// </summary>
    /// <param name="exchange">The exchange to publish to.</param>
    /// <param name="routingKey">The routing key for message routing.</param>
    /// <param name="message">The message payload (JSON).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync(string exchange, string routingKey, string message, CancellationToken cancellationToken = default);
}
