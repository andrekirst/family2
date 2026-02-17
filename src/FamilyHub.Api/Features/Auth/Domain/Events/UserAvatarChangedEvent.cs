using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Auth.Domain.Events;

/// <summary>
/// Domain event raised when a user's global avatar is set or changed.
/// </summary>
public sealed record UserAvatarChangedEvent(
    UserId UserId,
    AvatarId NewAvatarId,
    AvatarId? PreviousAvatarId,
    DateTime ChangedAt
) : DomainEvent;
