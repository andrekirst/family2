using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.EventChain.Infrastructure.Orchestrator;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Api.Features.EventChain.Application.EventHandlers;

/// <summary>
/// Observes all domain events to trigger matching chain definitions.
/// Registered as an IDomainEventObserver and invoked by DomainEventPublishingBehavior.
/// </summary>
public sealed class ChainTriggerHandler(
    IChainOrchestrator orchestrator,
    ILogger<ChainTriggerHandler> logger) : IDomainEventObserver
{
    public async Task OnEventPublishedAsync(IDomainEvent @event, CancellationToken ct = default)
    {
        logger.LogDebug(
            "Chain trigger handler received event: {EventType}, EventId={EventId}",
            @event.GetType().FullName,
            @event.EventId);

        await orchestrator.TryTriggerChainsAsync(@event);
    }
}
