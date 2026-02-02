using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Domain.Events;

/// <summary>
/// Domain event raised when a new family is created.
/// Triggers default calendar creation and welcome workflows.
/// </summary>
public sealed record FamilyCreatedEvent(
    FamilyId FamilyId,
    FamilyName Name,
    UserId OwnerId,
    DateTime CreatedAt
) : DomainEvent;
