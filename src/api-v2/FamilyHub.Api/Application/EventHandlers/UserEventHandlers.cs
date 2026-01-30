using FamilyHub.Api.Domain.Events;
using MediatR;

namespace FamilyHub.Api.Application.EventHandlers;

/// <summary>
/// Event handlers for user-related domain events.
/// Currently logs to console - can be extended to send emails, trigger notifications, etc.
/// </summary>

public class UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger)
    : INotificationHandler<UserRegisteredEvent>
{
    public Task Handle(UserRegisteredEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "User registered: {UserId}, Email: {Email}, VerificationToken: {Token}, At: {OccurredAt}",
            notification.UserId,
            notification.Email.Value,
            notification.EmailVerificationToken,
            notification.OccurredAt);

        // TODO: Send verification email (when email service is ready)
        // await _emailService.SendEmailVerificationAsync(notification.Email, notification.EmailVerificationToken);

        return Task.CompletedTask;
    }
}

public class UserLoggedInEventHandler(ILogger<UserLoggedInEventHandler> logger)
    : INotificationHandler<UserLoggedInEvent>
{
    public Task Handle(UserLoggedInEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "User logged in: {UserId}, At: {OccurredAt}",
            notification.UserId,
            notification.OccurredAt);

        return Task.CompletedTask;
    }
}

public class UserEmailVerifiedEventHandler(ILogger<UserEmailVerifiedEventHandler> logger)
    : INotificationHandler<UserEmailVerifiedEvent>
{
    public Task Handle(UserEmailVerifiedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "User email verified: {UserId}, At: {OccurredAt}",
            notification.UserId,
            notification.OccurredAt);

        // TODO: Send welcome email
        // await _emailService.SendWelcomeEmailAsync(notification.UserId);

        return Task.CompletedTask;
    }
}

public class PasswordResetRequestedEventHandler(ILogger<PasswordResetRequestedEventHandler> logger)
    : INotificationHandler<PasswordResetRequestedEvent>
{
    public Task Handle(PasswordResetRequestedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Password reset requested: {UserId}, Email: {Email}, Token: {Token}, At: {OccurredAt}",
            notification.UserId,
            notification.Email.Value,
            notification.ResetToken,
            notification.OccurredAt);

        // TODO: Send password reset email
        // await _emailService.SendPasswordResetEmailAsync(notification.Email, notification.ResetToken);

        return Task.CompletedTask;
    }
}

public class PasswordChangedEventHandler(ILogger<PasswordChangedEventHandler> logger)
    : INotificationHandler<PasswordChangedEvent>
{
    public Task Handle(PasswordChangedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Password changed: {UserId}, At: {OccurredAt}",
            notification.UserId,
            notification.OccurredAt);

        // TODO: Send security notification email
        // await _emailService.SendPasswordChangedNotificationAsync(notification.UserId);

        return Task.CompletedTask;
    }
}
