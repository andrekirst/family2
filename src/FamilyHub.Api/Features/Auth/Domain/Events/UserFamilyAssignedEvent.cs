using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Auth.Domain.Events;

/// <summary>
/// Domain event raised when a user is assigned to a family.
/// Triggers family membership workflows.
/// </summary>
public sealed record UserFamilyAssignedEvent(
    UserId UserId,
    FamilyId FamilyId,
    DateTime AssignedAt
) : DomainEvent;
