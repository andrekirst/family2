using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Domain.Events;

/// <summary>
/// Domain event raised when a new family is created.
/// Triggers event chain: calendar creation, onboarding tasks, welcome notifications.
/// </summary>
public sealed record FamilyCreatedEvent(
    FamilyId FamilyId,
    FamilyName FamilyName,
    UserId OwnerId,
    DateTime CreatedAt
) : DomainEvent;
