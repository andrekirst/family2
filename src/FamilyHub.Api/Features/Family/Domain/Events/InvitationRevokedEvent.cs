using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Domain.Events;

/// <summary>
/// Domain event raised when a family invitation is revoked by an admin/owner.
/// </summary>
public sealed record InvitationRevokedEvent(
    InvitationId InvitationId,
    FamilyId FamilyId
) : DomainEvent;
