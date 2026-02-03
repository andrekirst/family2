using FamilyHub.Api.Features.Auth.Domain.Events;

namespace FamilyHub.Api.Features.Auth.Application.EventHandlers;

/// <summary>
/// Handler for UserFamilyAssignedEvent.
/// Triggers family membership workflows and notifications.
/// </summary>
public static class UserFamilyAssignedEventHandler
{
    public static Task Handle(
        UserFamilyAssignedEvent @event,
        ILogger logger)
    {
        logger.LogInformation(
            "User assigned to family: UserId={UserId}, FamilyId={FamilyId}",
            @event.UserId.Value,
            @event.FamilyId.Value);

        // TODO: Send notification to family members about new member
        // TODO: Grant user access to family calendar/tasks
        // TODO: Create default user-specific tasks for family

        return Task.CompletedTask;
    }
}
