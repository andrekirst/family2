using FamilyHub.Api.Features.Family.Domain.Events;

namespace FamilyHub.Api.Features.Family.Application.EventHandlers;

/// <summary>
/// Handler for FamilyCreatedEvent.
/// Triggers default family setup workflows (calendar, tasks, etc.).
/// This is the foundation for event chain automation.
/// </summary>
public static class FamilyCreatedEventHandler
{
    public static Task Handle(
        FamilyCreatedEvent @event,
        ILogger logger)
    {
        logger.LogInformation(
            "Family created: FamilyId={FamilyId}, Name={Name}, OwnerId={OwnerId}",
            @event.FamilyId.Value,
            @event.Name.Value,
            @event.OwnerId.Value);

        // TODO: EVENT CHAIN - Create default family calendar
        // TODO: EVENT CHAIN - Create default shared shopping list
        // TODO: EVENT CHAIN - Create welcome tasks for family owner
        // TODO: Send notification to family owner about successful creation

        return Task.CompletedTask;
    }
}
