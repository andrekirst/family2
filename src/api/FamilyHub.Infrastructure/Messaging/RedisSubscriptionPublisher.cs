using HotChocolate.Subscriptions;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Infrastructure.Messaging;

/// <summary>
/// Implementation of <see cref="IRedisSubscriptionPublisher"/> using Hot Chocolate's ITopicEventSender.
/// </summary>
/// <remarks>
/// <para>
/// This publisher sends messages to Redis PubSub topics, which Hot Chocolate then delivers
/// to subscribed GraphQL clients via WebSocket connections. It follows the same logging and
/// error handling patterns as <see cref="RabbitMqPublisher"/> for consistency.
/// </para>
/// <para>
/// <strong>Architecture:</strong>
/// </para>
/// <code>
/// Domain Events (RabbitMQ) → Event Handlers → Redis PubSub → Hot Chocolate → WebSocket → Client
/// </code>
/// <para>
/// <strong>Key Design Decisions:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Errors are logged but not thrown (best-effort delivery)</description></item>
/// <item><description>Messages are ephemeral (not persisted)</description></item>
/// <item><description>Structured logging to Seq for observability</description></item>
/// </list>
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="RedisSubscriptionPublisher"/> class.
/// </remarks>
/// <param name="topicEventSender">Hot Chocolate's topic event sender for Redis PubSub.</param>
/// <param name="logger">Logger for structured logging to Seq.</param>
public sealed partial class RedisSubscriptionPublisher(
    ITopicEventSender topicEventSender,
    ILogger<RedisSubscriptionPublisher> logger) : IRedisSubscriptionPublisher
{
    private readonly ITopicEventSender _topicEventSender = topicEventSender;
    private readonly ILogger<RedisSubscriptionPublisher> _logger = logger;

    /// <inheritdoc />
    public async Task PublishAsync<TMessage>(
        string topicName,
        TMessage message,
        CancellationToken cancellationToken = default)
        where TMessage : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topicName);
        ArgumentNullException.ThrowIfNull(message);

        try
        {
            LogPublishingMessage(topicName, typeof(TMessage).Name);

            await _topicEventSender.SendAsync(topicName, message, cancellationToken);

            LogMessagePublished(topicName, typeof(TMessage).Name);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - subscription publishing is best-effort
            // We don't want to fail the primary operation (e.g., accepting invitation)
            // if subscription delivery fails
            LogPublishError(topicName, typeof(TMessage).Name, ex.Message);
        }
    }

    // High-performance structured logging using LoggerMessage attribute
    [LoggerMessage(LogLevel.Debug, "Publishing subscription message to topic {TopicName} (Type: {MessageType})")]
    partial void LogPublishingMessage(string topicName, string messageType);

    [LoggerMessage(LogLevel.Information, "Published subscription message to topic {TopicName} (Type: {MessageType})")]
    partial void LogMessagePublished(string topicName, string messageType);

    [LoggerMessage(LogLevel.Warning, "Failed to publish subscription message to topic {TopicName} (Type: {MessageType}): {ErrorMessage}")]
    partial void LogPublishError(string topicName, string messageType, string errorMessage);
}
