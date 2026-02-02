using FamilyHub.Api.Features.Family.Domain.Events;

namespace FamilyHub.Api.Features.Family.Application.EventHandlers;

/// <summary>
/// Handler for FamilyMemberAddedEvent.
/// Triggers member onboarding workflows and notifications.
/// </summary>
public static class FamilyMemberAddedEventHandler
{
    public static Task Handle(
        FamilyMemberAddedEvent @event,
        ILogger logger)
    {
        logger.LogInformation(
            "Member added to family: FamilyId={FamilyId}, UserId={UserId}",
            @event.FamilyId.Value,
            @event.UserId.Value);

        // TODO: Notify all existing family members about new member
        // TODO: Grant new member access to family calendar/tasks
        // TODO: EVENT CHAIN - Create introduction task for new member
        // TODO: Send welcome notification to new member

        return Task.CompletedTask;
    }
}
