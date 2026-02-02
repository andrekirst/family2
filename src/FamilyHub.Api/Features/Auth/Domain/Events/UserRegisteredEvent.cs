using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Auth.Domain.Events;

/// <summary>
/// Domain event raised when a new user registers via OAuth.
/// Triggers welcome workflows and onboarding tasks.
/// </summary>
public sealed record UserRegisteredEvent(
    UserId UserId,
    Email Email,
    UserName Name,
    ExternalUserId ExternalUserId,
    bool EmailVerified,
    DateTime RegisteredAt
) : DomainEvent;
