using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Domain.Events;

/// <summary>
/// Domain event raised when a family invitation is accepted.
/// </summary>
public sealed record InvitationAcceptedEvent(
    InvitationId InvitationId,
    FamilyId FamilyId,
    UserId AcceptedByUserId,
    FamilyRole Role
) : DomainEvent;
