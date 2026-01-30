using FamilyHub.Api.Domain.ValueObjects;

namespace FamilyHub.Api.Domain.Events;

/// <summary>
/// User was successfully registered.
/// Triggers: Email verification email
/// </summary>
public sealed record UserRegisteredEvent(
    UserId UserId,
    Email Email,
    string EmailVerificationToken) : DomainEvent;

/// <summary>
/// User successfully logged in.
/// Triggers: Logging, analytics
/// </summary>
public sealed record UserLoggedInEvent(
    UserId UserId) : DomainEvent;

/// <summary>
/// User's email was verified.
/// Triggers: Welcome email, onboarding flow
/// </summary>
public sealed record UserEmailVerifiedEvent(
    UserId UserId) : DomainEvent;

/// <summary>
/// User requested password reset.
/// Triggers: Password reset email
/// </summary>
public sealed record PasswordResetRequestedEvent(
    UserId UserId,
    Email Email,
    string ResetToken) : DomainEvent;

/// <summary>
/// User's password was changed.
/// Triggers: Security notification email
/// </summary>
public sealed record PasswordChangedEvent(
    UserId UserId) : DomainEvent;
