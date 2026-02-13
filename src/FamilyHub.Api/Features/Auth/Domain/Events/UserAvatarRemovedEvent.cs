using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Auth.Domain.Events;

/// <summary>
/// Domain event raised when a user's global avatar is removed.
/// </summary>
public sealed record UserAvatarRemovedEvent(
    UserId UserId,
    AvatarId RemovedAvatarId,
    DateTime RemovedAt
) : DomainEvent;
