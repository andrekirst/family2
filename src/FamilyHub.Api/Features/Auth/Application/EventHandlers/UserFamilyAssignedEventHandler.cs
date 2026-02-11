using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.Events;

namespace FamilyHub.Api.Features.Auth.Application.EventHandlers;

/// <summary>
/// Handler for UserFamilyAssignedEvent.
/// Triggers family membership workflows and notifications.
/// </summary>
public sealed class UserFamilyAssignedEventHandler(ILogger<UserFamilyAssignedEventHandler> logger)
    : IDomainEventHandler<UserFamilyAssignedEvent>
{
    public ValueTask Handle(
        UserFamilyAssignedEvent @event,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "User assigned to family: UserId={UserId}, FamilyId={FamilyId}",
            @event.UserId.Value,
            @event.FamilyId.Value);

        // TODO: Send notification to family members about new member
        // TODO: Grant user access to family calendar/tasks
        // TODO: Create default user-specific tasks for family

        return default;
    }
}
