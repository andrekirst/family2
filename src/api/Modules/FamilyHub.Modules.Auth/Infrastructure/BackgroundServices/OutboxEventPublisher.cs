using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Infrastructure.Messaging;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that publishes pending outbox events to RabbitMQ.
/// Implements reliable event publishing with exponential backoff retry.
/// </summary>
/// <remarks>
/// <para><strong>Retry Strategy:</strong></para>
/// <para>
/// Exponential backoff with max delay capped at 15 minutes.
/// Retries forever (no circuit breaker - events eventually succeed or require manual intervention).
/// </para>
/// <para><strong>Retry Schedule:</strong></para>
/// <list type="bullet">
/// <item>Attempt 1: Immediate</item>
/// <item>Attempt 2: 1 second</item>
/// <item>Attempt 3: 2 seconds</item>
/// <item>Attempt 4: 5 seconds</item>
/// <item>Attempt 5: 15 seconds</item>
/// <item>Attempt 6: 60 seconds (1 minute)</item>
/// <item>Attempt 7: 300 seconds (5 minutes)</item>
/// <item>Attempt 8+: 900 seconds (15 minutes) - forever</item>
/// </list>
/// </remarks>
public sealed partial class OutboxEventPublisher(
    IServiceProvider serviceProvider,
    ILogger<OutboxEventPublisher> logger) : BackgroundService
{
    private const int PollingIntervalSeconds = 5;
    private const int BatchSize = 100;
    private const int MaxRetryDelay = 900; // 15 minutes

    // Exponential backoff delays (in seconds)
    private static readonly int[] RetryDelays = [1, 2, 5, 15, 60, 300, MaxRetryDelay];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogOutboxEventPublisherStartedPollingEveryIntervalSeconds(logger, PollingIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingEventsAsync(stoppingToken);
            }
            catch (Exception)
            {
                LogErrorProcessingOutboxEventsWillRetryInIntervalSeconds(logger, PollingIntervalSeconds);
            }

            await Task.Delay(TimeSpan.FromSeconds(PollingIntervalSeconds), stoppingToken);
        }

        logger.LogInformation("Outbox Event Publisher stopped.");
    }

    private async Task ProcessPendingEventsAsync(CancellationToken cancellationToken)
    {
        // Create a scope for scoped dependencies
        using var scope = serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOutboxEventRepository>();
        var publisher = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // 1. Fetch pending events
        var pendingEvents = await repository.GetPendingEventsAsync(BatchSize, cancellationToken);

        if (pendingEvents.Count == 0)
        {
            return; // No events to process
        }

        LogProcessingCountPendingOutboxEvents(logger, pendingEvents.Count);

        foreach (var outboxEvent in pendingEvents)
        {
            try
            {
                // 2. Check if event should be delayed (exponential backoff)
                var requiredDelay = CalculateRetryDelay(outboxEvent.RetryCount);
                var timeSinceLastUpdate = DateTime.UtcNow - outboxEvent.UpdatedAt;

                if (timeSinceLastUpdate < requiredDelay)
                {
                    var remainingDelay = requiredDelay - timeSinceLastUpdate;
                    LogEventEventidRetryRetrycountNotReadyForRetryRemainingDelayRemainingdelay(logger, outboxEvent.Id, outboxEvent.RetryCount, remainingDelay);

                    // Skip this event for now - it will be processed in a future poll
                    continue;
                }

                // 3. Publish to RabbitMQ
                var exchange = "family-hub.events"; // TODO: Make configurable
                var routingKey = outboxEvent.EventType;

                await publisher.PublishAsync(exchange, routingKey, outboxEvent.Payload, cancellationToken);

                // 4. Mark as processed
                outboxEvent.MarkAsProcessed();
                await repository.UpdateAsync(outboxEvent, cancellationToken);

                LogPublishedOutboxEventEventidOfTypeEventtypeAggregateAggregatetypeAggregateid(logger, outboxEvent.Id, outboxEvent.EventType, outboxEvent.AggregateType, outboxEvent.AggregateId);
            }
            catch (Exception ex)
            {
                // 5. Mark as failed and increment retry count (but keep as Pending for automatic retry)
                outboxEvent.MarkAsFailedWithRetry(ex.Message);
                await repository.UpdateAsync(outboxEvent, cancellationToken);

                LogFailedToPublishOutboxEventEventidOfTypeEventtypeRetryCountRetrycountWillRetry(logger, outboxEvent.Id, outboxEvent.EventType, outboxEvent.RetryCount);
            }
        }

        // 6. Save all changes atomically
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Calculates retry delay based on retry count using exponential backoff.
    /// </summary>
    /// <param name="retryCount">Current retry count.</param>
    /// <returns>Delay before next retry. Returns TimeSpan.Zero if should retry immediately.</returns>
    private static TimeSpan CalculateRetryDelay(int retryCount)
    {
        if (retryCount == 0)
        {
            return TimeSpan.Zero; // First attempt - no delay
        }

        // Get delay from table (with max delay ceiling)
        var delayIndex = Math.Min(retryCount - 1, RetryDelays.Length - 1);
        var delaySeconds = RetryDelays[delayIndex];

        return TimeSpan.FromSeconds(delaySeconds);
    }

    [LoggerMessage(LogLevel.Information, "Outbox Event Publisher started. Polling every {interval} seconds.")]
    static partial void LogOutboxEventPublisherStartedPollingEveryIntervalSeconds(ILogger<OutboxEventPublisher> logger, int interval);

    [LoggerMessage(LogLevel.Debug, "Event {eventId} (retry {retryCount}) not ready for retry. Remaining delay: {remainingDelay}.")]
    static partial void LogEventEventidRetryRetrycountNotReadyForRetryRemainingDelayRemainingdelay(ILogger<OutboxEventPublisher> logger, OutboxEventId eventId, int retryCount, TimeSpan remainingDelay);

    [LoggerMessage(LogLevel.Information, "Published outbox event {eventId} of type {eventType} (aggregate: {aggregateType}/{aggregateId}).")]
    static partial void LogPublishedOutboxEventEventidOfTypeEventtypeAggregateAggregatetypeAggregateid(ILogger<OutboxEventPublisher> logger, OutboxEventId eventId, string eventType, string aggregateType, Guid aggregateId);

    [LoggerMessage(LogLevel.Error, "Failed to publish outbox event {eventId} of type {eventType}. Retry count: {retryCount}. Will retry with exponential backoff.")]
    static partial void LogFailedToPublishOutboxEventEventidOfTypeEventtypeRetryCountRetrycountWillRetry(ILogger<OutboxEventPublisher> logger, OutboxEventId eventId, string eventType, int retryCount);

    [LoggerMessage(LogLevel.Information, "Processing {count} pending outbox events.")]
    static partial void LogProcessingCountPendingOutboxEvents(ILogger<OutboxEventPublisher> logger, int count);

    [LoggerMessage(LogLevel.Error, "Error processing outbox events. Will retry in {interval} seconds.")]
    static partial void LogErrorProcessingOutboxEventsWillRetryInIntervalSeconds(ILogger<OutboxEventPublisher> logger, int interval);
}
