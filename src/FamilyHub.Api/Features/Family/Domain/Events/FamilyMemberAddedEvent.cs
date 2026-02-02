using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Domain.Events;

/// <summary>
/// Domain event raised when a member is added to a family.
/// Triggers member onboarding workflows and notifications.
/// </summary>
public sealed record FamilyMemberAddedEvent(
    FamilyId FamilyId,
    UserId UserId,
    DateTime AddedAt
) : DomainEvent;
