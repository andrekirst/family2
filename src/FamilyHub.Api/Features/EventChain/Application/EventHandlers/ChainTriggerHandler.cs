using FamilyHub.Common.Domain;
using FamilyHub.EventChain.Infrastructure.Orchestrator;

namespace FamilyHub.Api.Features.EventChain.Application.EventHandlers;

public static class ChainTriggerHandler
{
    public static async Task Handle(
        IDomainEvent @event,
        IChainOrchestrator orchestrator,
        ILogger logger)
    {
        logger.LogDebug(
            "Chain trigger handler received event: {EventType}, EventId={EventId}",
            @event.GetType().FullName,
            @event.EventId);

        await orchestrator.TryTriggerChainsAsync(@event);
    }
}
