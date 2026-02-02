using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Domain.Events;

/// <summary>
/// Domain event raised when a member is removed from a family.
/// Triggers cleanup workflows and notifications.
/// </summary>
public sealed record FamilyMemberRemovedEvent(
    FamilyId FamilyId,
    UserId UserId,
    DateTime RemovedAt
) : DomainEvent;
