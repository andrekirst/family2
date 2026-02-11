using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.Events;

namespace FamilyHub.Api.Features.Auth.Application.EventHandlers;

/// <summary>
/// Handler for UserFamilyRemovedEvent.
/// Triggers cleanup workflows when user leaves family.
/// </summary>
public sealed class UserFamilyRemovedEventHandler(ILogger<UserFamilyRemovedEventHandler> logger)
    : IDomainEventHandler<UserFamilyRemovedEvent>
{
    public ValueTask Handle(
        UserFamilyRemovedEvent @event,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "User removed from family: UserId={UserId}, PreviousFamilyId={PreviousFamilyId}",
            @event.UserId.Value,
            @event.PreviousFamilyId.Value);

        // TODO: Remove user access to family calendar/tasks
        // TODO: Notify remaining family members
        // TODO: Clean up user-specific data tied to family

        return default;
    }
}
