using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Auth.Domain.Events;

/// <summary>
/// Domain event raised when a user is removed from their family.
/// Triggers cleanup and notification workflows.
/// </summary>
public sealed record UserFamilyRemovedEvent(
    UserId UserId,
    FamilyId PreviousFamilyId,
    DateTime RemovedAt
) : DomainEvent;
