namespace FamilyHub.Modules.Auth.Infrastructure.Messaging;

/// <summary>
/// Stub implementation of RabbitMQ publisher for Phase 2.
/// Logs events instead of actually publishing to RabbitMQ.
/// </summary>
/// <remarks>
/// This stub allows the outbox pattern to be fully implemented and tested
/// without requiring real RabbitMQ infrastructure in Phase 2.
/// Replace with actual RabbitMQ implementation in Phase 5+.
/// </remarks>
public sealed partial class StubRabbitMqPublisher(ILogger<StubRabbitMqPublisher> logger) : IRabbitMqPublisher
{
    public Task PublishAsync(string exchange, string routingKey, string message, CancellationToken cancellationToken = default)
    {
        LogStubPublishedEventToExchangeRoutingkeyMessage(exchange, routingKey, message);

        return Task.CompletedTask;
    }

    [LoggerMessage(LogLevel.Information, "STUB: Published event to {exchange}/{routingKey}: {message}")]
    partial void LogStubPublishedEventToExchangeRoutingkeyMessage(string exchange, string routingKey, string message);
}
