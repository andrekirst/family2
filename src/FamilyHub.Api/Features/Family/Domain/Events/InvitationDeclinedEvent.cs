using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Domain.Events;

/// <summary>
/// Domain event raised when a family invitation is declined.
/// </summary>
public sealed record InvitationDeclinedEvent(
    InvitationId InvitationId,
    FamilyId FamilyId
) : DomainEvent;
