using FamilyHub.Api.Features.Auth.Domain.Events;

namespace FamilyHub.Api.Features.Auth.Application.EventHandlers;

/// <summary>
/// Handler for UserRegisteredEvent.
/// Triggers welcome workflows and onboarding tasks.
/// Wolverine discovers this handler by convention (static Handle method).
/// </summary>
public static class UserRegisteredEventHandler
{
    public static Task Handle(
        UserRegisteredEvent @event,
        ILogger logger)
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

        return Task.CompletedTask;
    }
}
