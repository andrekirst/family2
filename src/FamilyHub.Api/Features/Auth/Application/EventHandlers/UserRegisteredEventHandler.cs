using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.Events;

namespace FamilyHub.Api.Features.Auth.Application.EventHandlers;

/// <summary>
/// Handler for UserRegisteredEvent.
/// Triggers welcome workflows and onboarding tasks.
/// </summary>
public sealed class UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger)
    : IDomainEventHandler<UserRegisteredEvent>
{
    public ValueTask Handle(
        UserRegisteredEvent @event,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "User registered: UserId={UserId}, Email={Email}, Name={Name}, EmailVerified={EmailVerified}",
            @event.UserId.Value,
            @event.Email.Value,
            @event.Name.Value,
            @event.EmailVerified);

        // TODO: Implement welcome email workflow
        // TODO: Create onboarding tasks for new users
        // TODO: Initialize default user preferences

        return default;
    }
}
